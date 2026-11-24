using BillarPatrik.Data;
using BillarPatrik.Modelo;

namespace BillarPatrik.Vista
{
    public partial class MainPage : ContentPage
    {
        private readonly ConexionDB _db = new ConexionDB();
        private Mesa _mesaSeleccionada;
        private List<Producto> _productos;
        private string _trabajador;
        private IDispatcherTimer _timer;

        public MainPage(string nombreTrabajador)
        {
            InitializeComponent();
            _trabajador = nombreTrabajador;
            lblTrabajador.Text = $"Trabajador: {_trabajador}";
            _ = CargarDatos();
            ConfigurarTimer();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            bool narrow = width < 850;

            if (narrow)
            {
                rootGrid.RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                };
                rootGrid.ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star)
                };

                Grid.SetColumnSpan(HeaderPanel, 1);
                Grid.SetRow(MesaPanel, 1);
                Grid.SetColumn(MesaPanel, 0);
                Grid.SetColumnSpan(MesaPanel, 1);
                Grid.SetRow(DetailPanel, 2);
                Grid.SetColumn(DetailPanel, 0);
                Grid.SetColumnSpan(DetailPanel, 1);
                Grid.SetRow(footerGrid, 3);
                Grid.SetColumn(footerGrid, 0);
                Grid.SetColumnSpan(footerGrid, 1);

                layoutMesas.Span = width < 520 ? 1 : 2;
                MesaPanel.MinimumHeightRequest = 420;
            }
            else
            {
                rootGrid.RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                };
                rootGrid.ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(new GridLength(1.35, GridUnitType.Star))
                };

                Grid.SetColumnSpan(HeaderPanel, 2);
                Grid.SetRow(MesaPanel, 1);
                Grid.SetColumn(MesaPanel, 0);
                Grid.SetColumnSpan(MesaPanel, 1);
                Grid.SetRow(DetailPanel, 1);
                Grid.SetColumn(DetailPanel, 1);
                Grid.SetColumnSpan(DetailPanel, 1);
                Grid.SetRow(footerGrid, 2);
                Grid.SetColumn(footerGrid, 0);
                Grid.SetColumnSpan(footerGrid, 2);

                layoutMesas.Span = 2;
                MesaPanel.ClearValue(VisualElement.MinimumHeightRequestProperty);
            }
        }

        private void ConfigurarTimer()
        {
            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => { ActualizarTiemposVisuales(); };
            _timer.Start();
        }

        private void ActualizarTiemposVisuales()
        {
            if (cvMesas.ItemsSource is not List<Mesa> mesas) return;

            foreach (var mesa in mesas.Where(m => m.Ocupada))
            {
                if (mesa.Modalidad == "TIEMPO LIBRE")
                {
                    TimeSpan tiempo = DateTime.Now - mesa.HoraInicio;
                    mesa.TiempoTranscurrido = tiempo.ToString(@"hh\:mm\:ss");
                }
                else
                {
                    int segundosTotales = mesa.MinutosFijos * 60;
                    int segundosTranscurridos = (int)(DateTime.Now - mesa.HoraInicio).TotalSeconds;
                    int segundosRestantes = segundosTotales - segundosTranscurridos;
                    if (segundosRestantes < 0) segundosRestantes = 0;
                    mesa.TiempoTranscurrido = TimeSpan.FromSeconds(segundosRestantes).ToString(@"hh\:mm\:ss");
                }
            }

            if (_mesaSeleccionada != null && _mesaSeleccionada.Ocupada)
            {
                lblTiempoDetalle.Text = _mesaSeleccionada.TiempoTranscurrido;
            }
        }

        private async Task CargarDatos()
        {
            var mesas = await _db.ObtenerMesas();
            cvMesas.ItemsSource = mesas;

            _productos = await _db.ObtenerProductos();

            await ActualizarRecaudacion();
        }

        private async Task ActualizarRecaudacion()
        {
            var ventas = await _db.ObtenerVentasPorTrabajador(_trabajador);
            decimal total = ventas.Sum(v => v.TotalGeneral);
            lblRecaudacion.Text = $"{total:0.00} Bs";
        }

        private async void SeleccionarMesa(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Mesa mesa)
            {
                _mesaSeleccionada = mesa;
                MostrarDetalleMesa(mesa);

                if (!mesa.Ocupada)
                {
                    await IniciarMesa(mesa);
                }
            }
            else
            {
                stackDetalle.IsVisible = false;
                lblDetalleMesa.Text = "Seleccione una mesa para ver su detalle.";
                lblDetalleMesa.IsVisible = true;
                btnAgregarConsumoDetalle.IsVisible = false;
                btnCobrarDetalle.IsVisible = false;
            }
        }

        private void MostrarDetalleMesa(Mesa mesa)
        {
            if (!mesa.Ocupada)
            {
                stackDetalle.IsVisible = false;
                lblDetalleMesa.Text = $"Mesa {mesa.NumeroMesa} - LIBRE";
                lblDetalleMesa.IsVisible = true;
                btnAgregarConsumoDetalle.IsVisible = false;
                btnCobrarDetalle.IsVisible = false;
                return;
            }

            lblDetalleMesa.IsVisible = false;
            stackDetalle.IsVisible = true;
            lblClienteDetalle.Text = mesa.Cliente;
            lblTiempoDetalle.Text = mesa.TiempoTranscurrido;
            lblConsumoDetalle.Text = $"{mesa.ConsumoTotal:0.00} Bs";

            btnAgregarConsumoDetalle.IsVisible = true;
            btnCobrarDetalle.IsVisible = true;
        }

        private async Task IniciarMesa(Mesa mesa)
        {
            string cliente = await DisplayPromptAsync("Nuevo Cliente", "Ingrese el nombre del cliente:", "OK", "Cancelar", "Cliente");
            if (string.IsNullOrWhiteSpace(cliente)) return;

            string modalidad = await DisplayActionSheet("Modalidad", "Cancelar", null, "TIEMPO LIBRE", "TIEMPO FIJO");

            if (modalidad == "TIEMPO FIJO")
            {
                string horasStr = await DisplayPromptAsync("Tiempo Fijo", "Ingrese las HORAS:", "OK", "Cancelar", "0", keyboard: Keyboard.Numeric);
                if (!int.TryParse(horasStr, out int horas) || horas < 0) return;

                string minutosStr = await DisplayPromptAsync("Tiempo Fijo", "Ingrese los MINUTOS:", "OK", "Cancelar", "0", keyboard: Keyboard.Numeric);
                if (!int.TryParse(minutosStr, out int minutos) || minutos < 0) return;

                int totalMinutos = (horas * 60) + minutos;
                if (totalMinutos <= 0) return;
                mesa.MinutosFijos = totalMinutos;
            }

            mesa.Ocupada = true;
            mesa.Cliente = cliente;
            mesa.Modalidad = modalidad;
            mesa.HoraInicio = DateTime.Now;
            mesa.ConsumoTotal = 0;

            await _db.ActualizarMesa(mesa);
            await CargarDatos();
            MostrarDetalleMesa(mesa);
        }

        private async void AgregarConsumoDetalle(object sender, EventArgs e)
        {
            if (_mesaSeleccionada == null || !_mesaSeleccionada.Ocupada)
            {
                await DisplayAlert("Aviso", "Seleccione una mesa ocupada.", "OK");
                return;
            }

            var productos = await _db.ObtenerProductos();
            var nombres = productos.Select(p => $"{p.Nombre} - {p.Precio:0.00} Bs").ToArray();

            string seleccion = await DisplayActionSheet("Seleccione un producto", "Cancelar", null, nombres);
            if (seleccion == "Cancelar" || seleccion == null) return;

            var productoSeleccionado = productos.FirstOrDefault(p => seleccion.StartsWith(p.Nombre));
            if (productoSeleccionado == null) return;

            _mesaSeleccionada.ConsumoTotal += productoSeleccionado.Precio;
            await _db.ActualizarMesa(_mesaSeleccionada);

            await DisplayAlert("Consumo", $"Se agregó {productoSeleccionado.Nombre}. Consumo total: {_mesaSeleccionada.ConsumoTotal:0.00} Bs.", "OK");
            await CargarDatos();
            MostrarDetalleMesa(_mesaSeleccionada);
        }

        private async void CobrarMesaDetalle(object sender, EventArgs e)
        {
            if (_mesaSeleccionada == null || !_mesaSeleccionada.Ocupada)
            {
                await DisplayAlert("Aviso", "Seleccione una mesa ocupada para cobrar.", "OK");
                return;
            }

            TimeSpan tiempoTranscurrido = DateTime.Now - _mesaSeleccionada.HoraInicio;
            double minutosJugados = tiempoTranscurrido.TotalMinutes;

            decimal precioMesa = 0;
            string duracion = "";

            if (_mesaSeleccionada.Modalidad == "TIEMPO LIBRE")
            {
                if (minutosJugados < 15) minutosJugados = 15;
                precioMesa = (decimal)Math.Ceiling(minutosJugados) * 0.50m;
                duracion = $"{Math.Ceiling(minutosJugados)} MIN (Libre)";
            }
            else
            {
                int minutosContratados = _mesaSeleccionada.MinutosFijos;
                int minutosReales = (int)Math.Ceiling(minutosJugados);
                int minutosACobrar = minutosReales < minutosContratados ? minutosReales : minutosContratados;
                if (minutosACobrar < 15) minutosACobrar = 15;

                precioMesa = minutosACobrar * 0.50m;
                duracion = $"{minutosACobrar} MIN (Fijo)";
            }

            decimal consumo = _mesaSeleccionada.ConsumoTotal;
            decimal totalGeneral = precioMesa + consumo;

            var venta = new Venta
            {
                NumeroMesa = _mesaSeleccionada.NumeroMesa,
                Cliente = _mesaSeleccionada.Cliente,
                Fecha = DateTime.Now,
                Duracion = duracion,
                TotalMesa = precioMesa,
                TotalConsumo = consumo,
                TotalGeneral = totalGeneral,
                Trabajador = _trabajador,
                Entregado = false
            };

            await _db.GuardarVenta(venta);

            _mesaSeleccionada.Ocupada = false;
            _mesaSeleccionada.Cliente = "";
            _mesaSeleccionada.Modalidad = "";
            _mesaSeleccionada.ConsumoTotal = 0;
            _mesaSeleccionada.HoraInicio = DateTime.MinValue;
            _mesaSeleccionada.MinutosFijos = 0;
            _mesaSeleccionada.TiempoTranscurrido = "";
            await _db.ActualizarMesa(_mesaSeleccionada);

            await DisplayAlert("Cobro Exitoso",
                $"Mesa: {_mesaSeleccionada.NumeroMesa}\n" +
                $"Cliente: {venta.Cliente}\n" +
                $"Tiempo: {duracion}\n" +
                $"Mesa: {precioMesa:0.00} Bs\n" +
                $"Consumo: {consumo:0.00} Bs\n" +
                $"TOTAL: {totalGeneral:0.00} Bs", "OK");

            _mesaSeleccionada = null;
            stackDetalle.IsVisible = false;
            lblDetalleMesa.Text = "Seleccione una mesa para ver su detalle.";
            lblDetalleMesa.IsVisible = true;
            btnAgregarConsumoDetalle.IsVisible = false;
            btnCobrarDetalle.IsVisible = false;

            await CargarDatos();
        }

        private async void CerrarTurno(object sender, EventArgs e)
        {
            var ventas = await _db.ObtenerVentasPorTrabajador(_trabajador);
            decimal totalMesas = ventas.Sum(v => v.TotalMesa);
            decimal totalConsumo = ventas.Sum(v => v.TotalConsumo);
            decimal totalGeneral = ventas.Sum(v => v.TotalGeneral);

            bool confirmar = await DisplayAlert("CIERRE DE CAJA",
                $"Trabajador: {_trabajador}\n" +
                $"Total Mesas: {totalMesas:0.00} Bs\n" +
                $"Total Consumo: {totalConsumo:0.00} Bs\n" +
                $"TOTAL A ENTREGAR: {totalGeneral:0.00} Bs\n\n" +
                $"¿Confirma el cierre de caja?", "Sí", "No");

            if (confirmar)
            {
                await _db.CerrarCaja(_trabajador);
                await DisplayAlert("Caja Cerrada", "El turno ha sido cerrado y la caja entregada al administrador.", "OK");
                await CargarDatos();
            }
        }

        private async void CerrarSesion(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}