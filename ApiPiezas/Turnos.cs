namespace ApiPiezas
{
    public class Turnos
    {
        public string Pin { get; set; }
        public string UsuarioSesion {  get; set; }
        public DateTime InicioDeSesion { get; set; }
        public DateTime FinDeSesion { get; set; }
        public TimeSpan TiempoDeSesion => FinDeSesion - InicioDeSesion;
        public string TiempoFormateado => string.Format("{0:D2}h {1:D2}m {2:D2}s",
                    (int)TiempoDeSesion.TotalHours,
                    TiempoDeSesion.Minutes,
                    TiempoDeSesion.Seconds);
    }
}
