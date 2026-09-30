using SailorMoon.Player;
using UnityEngine;
using UnityEngine.UI;

namespace SailorMoon.UI
{
    /// <summary>
    /// Corazones y barra del especial. Provisional: la interfaz definitiva se
    /// diseña en la Fase 3; esta sirve para jugar y afinar el combate.
    ///
    /// Cuando la barra se llena, brilla y late: «es imposible no darse cuenta»
    /// (GDD §5.4).
    /// </summary>
    public class CombatHud : MonoBehaviour
    {
        [SerializeField] PlayerCombat _player;
        [SerializeField] Image[] _hearts;
        [SerializeField] Image _specialFill;
        [SerializeField] RectTransform _specialButton;
        [SerializeField] Color _heartOn = new(1f, 0.35f, 0.55f);
        [SerializeField] Color _heartOff = new(1f, 1f, 1f, 0.25f);
        [SerializeField] Color _barCharging = new(0.95f, 0.75f, 0.95f);
        [SerializeField] Color _barReady = new(1f, 0.9f, 0.35f);

        void Update()
        {
            if (_player == null) return;
            var h = _player.Health;
            for (int i = 0; i < _hearts.Length; i++)
            {
                _hearts[i].gameObject.SetActive(i < h.Max);
                _hearts[i].color = i < h.Current ? _heartOn : _heartOff;
            }

            bool ready = _player.SpecialReady;
            _specialFill.fillAmount = _player.Special01;
            float pulse = ready ? 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.08f : 1f;
            _specialFill.color = ready ? _barReady : _barCharging;
            _specialFill.transform.localScale = new Vector3(1f, pulse, 1f);
            if (_specialButton)
                _specialButton.localScale = Vector3.one * (ready ? pulse * 1.05f : 0.9f);
        }
    }
}
