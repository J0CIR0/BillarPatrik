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
        public string Nombre { get; set; }
        public string UsuarioLogin { get; set; }
        public string Contrasena { get; set; }
        public string Rol { get; set; }
    }

    [Table("Mesas")]
    public class Mesa : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int NumeroMesa { get; set; }
        public int TipoMesaId { get; set; }
        public string TipoMesaNombre { get; set; } = "";

        private bool _ocupada;
        public bool Ocupada
        {
            get => _ocupada;
            set
            {
                _ocupada = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BordeMesa));
                OnPropertyChanged(nameof(ImagenMesa));
                OnPropertyChanged(nameof(MostrarMesaLibre));
                OnPropertyChanged(nameof(MostrarMesaOcupada));
            }
        }

        [Ignore]
        public Color BordeMesa => Ocupada ? Color.FromArgb("#FF4C4C") : Color.FromArgb("#35C985");

        [Ignore]
        public string ImagenMesa => Ocupada ? "mesa_ocupada.png" : "mesa_libre.png";

        [Ignore]
        public bool MostrarMesaLibre => !Ocupada;

        [Ignore]
        public bool MostrarMesaOcupada => Ocupada;

        private string _cliente;
        public string Cliente
        {
            get => _cliente;
            set { _cliente = value; OnPropertyChanged(); }
        }

        private string _modalidad;
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

        private string _tiempoTranscurrido;
        [Ignore]
        public string TiempoTranscurrido
        {
            get => _tiempoTranscurrido;
            set { _tiempoTranscurrido = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Table("Productos")]
    public class Producto
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = "";
        public string ImagenRuta { get; set; } = "";
        public int UnidadesPorPaquete { get; set; } = 1;
        public int CajasCompra { get; set; }
        public int UnidadesPorCaja { get; set; } = 1;
        public decimal PrecioCompraCaja { get; set; }
        [Ignore]
        public int TotalUnidadesCompradas => CajasCompra * UnidadesPorCaja;
        [Ignore]
        public decimal CostoTotalCompra => CajasCompra * PrecioCompraCaja;
        public decimal PrecioCompraPaquete { get; set; }
        public decimal PrecioVentaUnidad { get; set; }
        public decimal PrecioCompraUnidad => UnidadesPorPaquete > 0
            ? PrecioCompraPaquete / UnidadesPorPaquete
            : 0;
        public decimal GananciaUnidad => PrecioVentaUnidad - PrecioCompraUnidad;
        [Ignore]
        public string ResumenContable => $"Costo {PrecioCompraUnidad:0.00} Bs · Venta {PrecioVentaUnidad:0.00} Bs · Ganancia {GananciaUnidad:0.00} Bs";
    }

    [Table("TiposMesa")]
    public class TipoMesa
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public decimal PrecioPorHora { get; set; }
        public bool Activo { get; set; } = true;
    }

    [Table("CategoriasProducto")]
    public class CategoriaProducto
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public bool Activo { get; set; } = true;
    }

    [Table("ConsumosMesa")]
    public class ConsumoMesa
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int MesaId { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = "";
        public int Cantidad { get; set; } = 1;
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioCompraUnitario { get; set; }
    }

    [Table("VentasProducto")]
    public class VentaProducto
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioCompraUnitario { get; set; }
        [Ignore]
        public decimal Subtotal => Cantidad * PrecioUnitario;
        [Ignore]
        public decimal Ganancia => Cantidad * (PrecioUnitario - PrecioCompraUnitario);
    }

    [Table("CierresCaja")]
    public class CierreCaja
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Trabajador { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime Fecha { get; set; }
        public decimal TotalMesas { get; set; }
        public decimal TotalConsumo { get; set; }
        public decimal TotalGeneral { get; set; }
        public int CantidadVentas { get; set; }
        public string IdsVentas { get; set; } = "";
    }

    [Table("Ventas")]
    public class Venta
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int NumeroMesa { get; set; }
        public string Cliente { get; set; }
        public DateTime Fecha { get; set; }
        public string Duracion { get; set; }
        public decimal TotalMesa { get; set; }
        public decimal TotalConsumo { get; set; }
        public decimal TotalGeneral { get; set; }
        public string Trabajador { get; set; }
        public bool Entregado { get; set; }
    }

    [Table("ComprasProducto")]
    public class CompraProducto
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = "";
        public int Cajas { get; set; }
        public int UnidadesPorCaja { get; set; }
        public int TotalUnidades { get; set; }
        public decimal PrecioPorCaja { get; set; }
        public decimal CostoTotal { get; set; }
        public decimal PrecioVentaUnidad { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = "";
    }

    [Table("Auditoria")]
    public class RegistroAuditoria
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = "";
        public string Rol { get; set; } = "";
        public string Accion { get; set; } = "";
        public string Entidad { get; set; } = "";
        public string Detalle { get; set; } = "";
    }
}