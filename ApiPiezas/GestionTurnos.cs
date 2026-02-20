using Serilog;
using System.Text.Json;

namespace ApiPiezas
{
    public static class GestionTurnos
    {
        private static readonly Dictionary<string, (string Nombre, DateTime Inicio)> _sesionesActivas = new();

        public static void RegistrarInicio(string pin, string nombre)
        {
            _sesionesActivas[pin] = (nombre, DateTime.Now);
            Log.Warning("🚀 SESIÓN INICIADA: {Usuario} (PIN: {Pin})", nombre, pin);
        }

        public static void RegistrarFin(string pin, string nombre)
        {
            if (_sesionesActivas.TryGetValue(pin, out var datos))
            {
                var resumen = new Turnos
                {
                    Pin = pin,
                    UsuarioSesion = datos.Nombre,
                    InicioDeSesion = datos.Inicio,
                    FinDeSesion = DateTime.Now
                };

                string carpetaSesiones = @"C:\pruebas\sesiones";
                if (!Directory.Exists(carpetaSesiones)) Directory.CreateDirectory(carpetaSesiones);

                string nombreArchivo = $"turno_{datos.Nombre}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string rutaCompleta = Path.Combine(carpetaSesiones, nombreArchivo);
                string json = JsonSerializer.Serialize(resumen, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(rutaCompleta, json);

                string rutaHistorial = Path.Combine(carpetaSesiones, "historial_fichajes.txt");
                string nuevaLinea = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] - Usuario: {datos.Nombre} | PIN: {pin} | Duración: {resumen.TiempoFormateado}{Environment.NewLine}";

                File.AppendAllText(rutaHistorial, nuevaLinea);

                Log.Warning("🚪 CIERRE Y GUARDADO: {Usuario} | Duración: {Tiempo}",
                    resumen.UsuarioSesion, resumen.TiempoFormateado);

                _sesionesActivas.Remove(pin);
            }
        }
    }
}
