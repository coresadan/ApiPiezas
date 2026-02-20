using ApiPiezas;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Telegram.Alternative;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Information()
             .Enrich.WithProperty("NombrePrograma", "🌐 API-SANTOS-DB")
             .WriteTo.Async(wt => wt.Console())
             .WriteTo.Async(wt => wt.Telegram(
                 botToken: "8572448307:AAEpWviIJ0qqd1YPBXysRjl2SpsXmUprVIw",
                 chatId: "5688537233",
                 outputTemplate: "{NombrePrograma} | {Level:u3} | 📝 {Message:lj}{NewLine}{Exception}"
             ))
             .WriteTo.Async(wt => wt.File(
                 path: "C:\\pruebas\\log\\log.log",
                 rollingInterval: RollingInterval.Day,
                 outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
             ))
             .CreateLogger();

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var app = builder.Build();


        string carpetaRaiz = @"C:\pruebas";
        string carpetaUsuarios = Path.Combine(carpetaRaiz, "Usuarios");
        string rutaUsuariosJson = Path.Combine(carpetaUsuarios, "usuarios.json");
        string nombreArchivo = $"sesion{DateTime.Now:yyyyMMdd}.log";

        if (!Directory.Exists(carpetaUsuarios)) Directory.CreateDirectory(carpetaUsuarios);

        // Configure the HTTP request pipeline.

        app.MapOpenApi();

        app.UseAuthorization();

        app.MapControllers();




        app.MapGet("/Usuarios", async () =>
        {
            try
            {
                if (!File.Exists(rutaUsuariosJson)) return Results.NotFound("No hay archivo de usuarios.");
                string contenido = await File.ReadAllTextAsync(rutaUsuariosJson);
                var usuarios = JsonSerializer.Deserialize<List<Usuario>>(contenido,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Results.Ok(usuarios);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error al leer: {ex.Message}");
            }
        });


        app.MapGet("/Hola", () => "Prueba");

        app.MapGet("/Piezas", () =>
        {
            var pieza = new CaracteristicasDePiezas()
            {
                Nombre = "Pieza 1",
                Color = "Rojo",
                Largo = 1000,
                Ancho = 600
            };

            return Results.Ok(pieza);
        });

        app.MapGet("/Piezas/", () =>
        {

            List<CaracteristicasDePiezas> listadoPiezas = new List<CaracteristicasDePiezas>();

            listadoPiezas.Add(new CaracteristicasDePiezas()
            {
                Nombre = "Pieza 1",
                Color = "Rojo",
                Largo = 1000,
                Ancho = 600
            });

            listadoPiezas.Add(new CaracteristicasDePiezas()
            {
                Nombre = "Pieza 2",
                Color = "Rojo",
                Largo = 2000,
                Ancho = 600
            });

            listadoPiezas.Add(new CaracteristicasDePiezas()
            {
                Nombre = "Pieza 1",
                Color = "Rojo",
                Largo = 1000,
                Ancho = 600
            });

            ;
            return Results.Ok(listadoPiezas);
        });


        app.MapGet("/DimeListas/", () =>
        {
            var carpeta = new DirectoryInfo(carpetaRaiz);
            List<string> Ficheros = new List<string>();

            foreach (var i in carpeta.GetFiles())
            {
                Ficheros.Add(i.Name);
            }
            return Results.Ok(Ficheros);


        });

        app.MapGet("/DimePiezasLista/{nombreLista}", (string nombreLista) =>
        {
            string ruta = Path.Combine(carpetaRaiz, nombreLista);
            if (!File.Exists(ruta)) return Results.NotFound("Lista no encontrada.");
            string contenido = File.ReadAllText(ruta);
            var piezas = JsonSerializer.Deserialize<List<CaracteristicasDePiezas>>(contenido,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Results.Ok(piezas.Select(p => new { p.Nombre, p.Color, p.Largo, p.Ancho, Estado = "Importado" }));
        });

        app.MapGet("/Piezas/{color}/{largo}/{ancho}", (string color, int largo, int ancho) =>
        {
            var pieza = new CaracteristicasDePiezas()
            {
                Nombre = "Pieza 1",
                Color = color,
                Largo = largo,
                Ancho = ancho
            };

            return Results.Ok(pieza);
        });


        app.MapPost("/usuarios/guardar", (List<Usuario> listaActualizada) => {
            try
            {
                string json = JsonSerializer.Serialize(listaActualizada, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(rutaUsuariosJson, json);

                Log.Information("✅ Base de datos de usuarios actualizada desde el Administrador.");
                return Results.Ok("Usuarios guardados en el servidor");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "💥 Error al intentar guardar usuarios en el servidor");
                return Results.Problem("Error al guardar en el disco del servidor");
            }
        });

        app.MapPost("/turnos/iniciar", (Usuario user) =>
        {
            GestionTurnos.RegistrarInicio(user.Pin, user.Nombre);
            return Results.Ok($"Turno iniciado para {user.Nombre}");
        });

        app.MapPost("/turnos/finalizar", (Usuario user) =>
        {
            GestionTurnos.RegistrarFin(user.Pin, user.Nombre);
            return Results.Ok($"Turno finalizado para {user.Nombre}");
        });

        try
        {
            Log.Information("🚀 Servidor en línea. Listo para compartir piezas y usuarios desde la base de datos.");
            app.Run();
        }

        catch (Exception ex)
        {
            Log.Error(ex, "💥 El servidor se ha detenido por un error inesperado.");
        }

        finally
        {
            Log.Warning("🔌 Desconectando servidor... Vaciando colas de logs.");
            Log.CloseAndFlush(); // Esto asegura que Serilog envíe los últimos mensajes antes de cerrarse
        }

}
}