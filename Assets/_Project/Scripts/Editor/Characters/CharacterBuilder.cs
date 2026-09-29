using System.Linq;
using SailorMoon.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace SailorMoon.EditorTools.Characters
{
    /// <summary>
    /// Monta el Animator Controller compartido y el prefab de cada Sailor.
    ///
    /// Se ejecuta UNA vez (o cuando se quiera rehacer desde cero). Lo que sale
    /// —el .controller y el .prefab— es un asset normal que se edita a mano en
    /// Unity: la ventana Animator para los estados, el Inspector para el resto.
    /// Esto es solo el andamio para no montarlo con veinte clics.
    ///
    /// Un solo controller para las once: todas comparten esqueleto Humanoid y
    /// animaciones (GDD §5.5). Unity reajusta los clips a la altura de cada una.
    /// </summary>
    static class CharacterBuilder
    {
        const string AnimFolder = "Assets/_Project/Art/Animations";
        const string ControllerPath = AnimFolder + "/SailorLocomotion.controller";
        const string CharFolder = "Assets/_Project/Art/Characters";
        const string PrefabFolder = "Assets/_Project/Prefabs/Characters";

        [MenuItem("Sailor Moon/Montaje/Animator de las Sailor")]
        public static AnimatorController BuildController()
        {
            // Se rehace DENTRO del asset existente: borrarlo y crearlo de nuevo
            // le cambia el GUID y los prefabs que lo usan se quedan sin él.
            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (ctrl == null)
                ctrl = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            else
                Clear(ctrl);
            ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            // `parameters` devuelve una copia: hay que reasignar el array.
            var ps = ctrl.parameters;
            ps.First(p => p.name == "Grounded").defaultBool = true;
            ctrl.parameters = ps;

            var sm = ctrl.layers[0].stateMachine;

            var loco = ctrl.CreateBlendTreeInController("Locomotion", out BlendTree tree, 0);
            tree.blendParameter = "Speed";
            tree.useAutomaticThresholds = false;
            tree.AddChild(Clip("idle"), 0f);
            tree.AddChild(Clip("walk"), 0.37f); // 2,4 m/s de 6,5: andar
            tree.AddChild(Clip("run"), 1f);
            sm.defaultState = loco;

            var jumpStart = State(sm, "JumpStart", "jump_start", new Vector3(300, 120));
            var jumpLoop = State(sm, "JumpLoop", "jump_loop", new Vector3(550, 120));
            var land = State(sm, "Land", "land", new Vector3(550, 0));

            // Saltar desde cualquier sitio.
            var toJump = sm.AddAnyStateTransition(jumpStart);
            toJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
            toJump.duration = 0.05f;
            toJump.canTransitionToSelf = false;

            // Del impulso a la caída: en cuanto el cuerpo empieza a bajar, o
            // al acabar el clip si el salto fuera más alto que el impulso.
            var startToLoop = jumpStart.AddTransition(jumpLoop);
            startToLoop.AddCondition(AnimatorConditionMode.Less, 0f, "VerticalSpeed");
            startToLoop.duration = 0.15f;
            var startEnds = jumpStart.AddTransition(jumpLoop);
            startEnds.hasExitTime = true;
            startEnds.exitTime = 0.95f;
            startEnds.duration = 0.15f;

            // Caer de un borde sin haber saltado.
            var fall = loco.AddTransition(jumpLoop);
            fall.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");
            fall.AddCondition(AnimatorConditionMode.Less, -3f, "VerticalSpeed");
            fall.duration = 0.15f;

            foreach (var from in new[] { jumpStart, jumpLoop })
            {
                var t = from.AddTransition(land);
                t.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
                t.AddCondition(AnimatorConditionMode.Less, 0.1f, "VerticalSpeed");
                t.duration = 0.05f;
            }

            // Si aterriza corriendo, no se para a amortiguar: sigue.
            var landRun = land.AddTransition(loco);
            landRun.AddCondition(AnimatorConditionMode.Greater, 0.3f, "Speed");
            landRun.duration = 0.1f;
            var landIdle = land.AddTransition(loco);
            landIdle.hasExitTime = true;
            landIdle.exitTime = 0.75f;
            landIdle.duration = 0.2f;

            EditorUtility.SetDirty(ctrl);
            AssetDatabase.SaveAssets();
            return ctrl;
        }

        static void Clear(AnimatorController ctrl)
        {
            var sm = ctrl.layers[0].stateMachine;
            foreach (var t in sm.anyStateTransitions) sm.RemoveAnyStateTransition(t);
            foreach (var st in sm.states) sm.RemoveState(st.state);
            foreach (var p in ctrl.parameters) ctrl.RemoveParameter(p);
            // Los blend trees viejos quedan como sub-assets huérfanos.
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(ControllerPath))
                if (o is BlendTree bt) Object.DestroyImmediate(bt, true);
        }

        static AnimatorState State(AnimatorStateMachine sm, string name, string clip, Vector3 pos)
        {
            var s = sm.AddState(name, pos);
            s.motion = Clip(clip);
            return s;
        }

        static AnimationClip Clip(string name)
        {
            string path = $"{AnimFolder}/Humanoid/{name}.fbx";
            var clip = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(c => !c.name.StartsWith("__preview"));
            if (clip == null) Debug.LogError($"[CharacterBuilder] No hay clip en {path}");
            return clip;
        }

        [MenuItem("Sailor Moon/Montaje/Prefab de Serena (jugable)")]
        public static GameObject BuildSerena() => BuildPlayable("Serena", outlineWidth: 0.004f);

        /// <summary>
        /// Una Sailor jugable: raíz con la física y la lógica, y dentro el
        /// modelo con su Animator. La raíz no rota con la animación ni la
        /// animación mueve la raíz: cada una tiene su trabajo.
        /// </summary>
        public static GameObject BuildPlayable(string name, float outlineWidth)
        {
            var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath) ?? BuildController();
            string modelPath = $"{CharFolder}/{name}/{name}.fbx";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            var avatar = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().FirstOrDefault();

            var root = new GameObject(name);
            var cc = root.AddComponent<CharacterController>();
            cc.height = 1.5f;
            cc.radius = 0.28f;
            cc.center = new Vector3(0, 0.77f, 0);
            cc.stepOffset = 0.35f;
            cc.slopeLimit = 50f;
            cc.skinWidth = 0.03f;
            root.AddComponent<InputReader>();
            root.AddComponent<PlayerMotor>();
            root.AddComponent<PlayerAnimator>();

            var target = new GameObject("CameraTarget").transform;
            target.SetParent(root.transform, false);
            target.localPosition = new Vector3(0, 1.25f, 0);

            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = "Model";
            visual.transform.SetParent(root.transform, false);
            var animator = visual.GetComponent<Animator>() ?? visual.AddComponent<Animator>();
            animator.runtimeAnimatorController = ctrl;
            animator.avatar = avatar;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            var mat = ToonMaterial(name, outlineWidth);
            foreach (var smr in visual.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                smr.sharedMaterials = Enumerable.Repeat(mat, smr.sharedMesh.subMeshCount).ToArray();
                // La caja de un personaje con esqueleto se calcula en reposo; si
                // una animación saca el cuerpo de ahí, se deja de dibujar según
                // el ángulo (trampa 11 de Godot). Se agranda de antemano.
                var b = smr.localBounds;
                b.Expand(b.size.magnitude * 0.5f);
                smr.localBounds = b;
                smr.updateWhenOffscreen = false;
            }

            AddHairChain(visual.transform, "Hair_L_1");
            AddHairChain(visual.transform, "Hair_R_1");

            System.IO.Directory.CreateDirectory(PrefabFolder);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{name}.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void AddHairChain(Transform model, string rootBone)
        {
            var bone = model.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == rootBone);
            if (bone == null) return;
            var chain = model.gameObject.AddComponent<SpringBoneChain>();
            var so = new SerializedObject(chain);
            so.FindProperty("_root").objectReferenceValue = bone;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// El material de un personaje: un asset propio, editable, con su textura.
        public static Material ToonMaterial(string name, float outlineWidth)
        {
            string path = $"{CharFolder}/{name}/{name}_Toon.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;
            mat = new Material(Shader.Find("SailorMoon/Toon"));
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{CharFolder}/{name}/{name}_Albedo.png"));
            mat.SetFloat("_OutlineWidth", outlineWidth);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }
    }
}
