using UnityEditor;

namespace SailorMoon.EditorTools
{
    /// <summary>
    /// Deja el puente del MCP (CoplayDev unity-mcp) como lo espera el
    /// <c>.mcp.json</c> de la raíz del repo: transporte stdio y sin telemetría.
    ///
    /// POR QUÉ EXISTE: el paquete guarda su configuración en EditorPrefs, que
    /// son de la máquina y no del proyecto. Sin esto, cada máquina nueva
    /// arrancaría en HTTP (su valor por defecto) y Claude Code, que lanza el
    /// servidor por stdio, no encontraría a Unity escuchando en el 6400.
    ///
    /// Con stdio, Claude Code arranca el servidor Python y este se conecta al
    /// editor abierto. No hay que pulsar «Start server» en ninguna ventana.
    /// </summary>
    [InitializeOnLoad]
    static class McpDefaults
    {
        const string AppliedKey = "SailorMoon.McpDefaultsApplied.v1";

        static McpDefaults()
        {
            if (EditorPrefs.GetBool(AppliedKey, false)) return;

            EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", false);
            EditorPrefs.SetBool("MCPForUnity.TelemetryDisabled", true);
            EditorPrefs.SetBool(AppliedKey, true);
        }
    }
}
