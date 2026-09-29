#!/usr/bin/env -S uv run --quiet --script
# /// script
# requires-python = ">=3.10"
# dependencies = ["mcp<2"]
# ///
"""Habla con el editor de Unity abierto a través del MCP, desde la terminal.

Para qué: un agente que tenga el MCP cargado (sesión abierta en esta carpeta,
con .mcp.json) no lo necesita. Esto es para los demás casos — un agente de
otra carpeta, un script, o comprobar a mano que el puente responde — con el
MISMO servidor y la misma versión que usa el .mcp.json.

  ./tools/unity_mcp.py herramientas              # lista de herramientas
  ./tools/unity_mcp.py codigo comprobacion.cs                # ejecuta C# en el editor
  ./tools/unity_mcp.py llamar read_console '{"action":"get","count":20}'
  ./tools/unity_mcp.py llamar execute_menu_item '{"menu_path":"Sailor Moon/Configurar proyecto"}'

Requisito: Unity abierto con este proyecto (el puente escucha en el 6400).
"""
import asyncio
import json
import sys

from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

SERVIDOR = StdioServerParameters(
    command="/opt/homebrew/bin/uvx",
    args=["--from", "mcpforunityserver==10.2.0", "mcp-for-unity", "--transport", "stdio"],
    env={"DISABLE_TELEMETRY": "true", "UNITY_MCP_DISABLE_TELEMETRY": "true",
         "PATH": "/opt/homebrew/bin:/usr/bin:/bin"},
)


async def main(argv):
    async with stdio_client(SERVIDOR) as (r, w):
        async with ClientSession(r, w) as s:
            await s.initialize()
            if argv[0] == "herramientas":
                for t in (await s.list_tools()).tools:
                    print(f"{t.name:32} {(t.description or '').splitlines()[0][:100]}")
                return
            if argv[0] == "esquema":
                for t in (await s.list_tools()).tools:
                    if t.name == argv[1]:
                        print(json.dumps(t.inputSchema, indent=2, ensure_ascii=False))
                return
            if argv[0] == "codigo":
                # Cuerpo de método C# en un archivo (o «-» para stdin): se
                # ejecuta en el editor y lo que devuelva con `return` se imprime.
                codigo = sys.stdin.read() if argv[1] == "-" else open(argv[1]).read()
                res = await s.call_tool("execute_code", {"action": "execute", "code": codigo})
                for c in res.content:
                    texto = getattr(c, "text", str(c))
                    try:
                        datos = json.loads(texto)
                        print(datos.get("data", {}).get("result", texto) if datos.get("success") else texto)
                    except (ValueError, AttributeError):
                        print(texto)
                if res.isError:
                    sys.exit(1)
                return
            if argv[0] == "llamar":
                args = json.loads(argv[2]) if len(argv) > 2 else {}
                res = await s.call_tool(argv[1], args)
                for c in res.content:
                    print(getattr(c, "text", c))
                if res.isError:
                    sys.exit(1)
                return
            sys.exit(__doc__)


if __name__ == "__main__":
    asyncio.run(main(sys.argv[1:] or ["?"]))
