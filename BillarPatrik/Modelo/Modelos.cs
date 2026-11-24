using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BillarPatrik.Modelo
{
    [Table("Usuarios")]
    public class Usuario
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string UsuarioLogin { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    [Table("Mesas")]
    public class Mesa : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int NumeroMesa { get; set; }

        private bool _ocupada;
        public bool Ocupada
        {
            get => _ocupada;
            set { _ocupada = value; OnPropertyChanged(); }
        }

        private string _cliente = string.Empty;
        public string Cliente
        {
            get => _cliente;
            set { _cliente = value; OnPropertyChanged(); }
        }

        private string _modalidad = string.Empty;
        public string Modalidad
        {
            get => _modalidad;
            set { _modalidad = value; OnPropertyChanged(); }
        }

        public DateTime HoraInicio { get; set; }
        public int MinutosFijos { get; set; }

        private decimal _consumoTotal;
        public decimal ConsumoTotal
        {
            get => _consumoTotal;
            set { _consumoTotal = value; OnPropertyChanged(); }
        }

        private string _tiempoTranscurrido = string.Empty;
        [Ignore]
        public string TiempoTranscurrido
        {
            get => _tiempoTranscurrido;
            set { _tiempoTranscurrido = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Table("Productos")]
    public class Producto
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }

    [Table("Ventas")]
    public class Venta
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int NumeroMesa { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Duracion { get; set; } = string.Empty;
        public decimal TotalMesa { get; set; }
        public decimal TotalConsumo { get; set; }
        public decimal TotalGeneral { get; set; }
        public string Trabajador { get; set; } = string.Empty;
        public bool Entregado { get; set; }
    }
}