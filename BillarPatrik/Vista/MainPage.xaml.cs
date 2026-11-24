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
        private List<TipoMesa> _tarifasMesa = new();
        private readonly SignalRService _signalRService = new();
        private bool _eventosSignalRRegistrados;

        private sealed class ConsumoDetalle
        {
            public string Nombre { get; init; } = "";
            public string Detalle { get; init; } = "";
            public string Subtotal { get; init; } = "";
            public ImageSource Imagen { get; init; } = ImageSource.FromFile("icono_consumo.png");
        }

        private sealed class ProductoConsumoRow
        {
            public Producto Producto { get; init; } = new();
            public string Nombre { get; init; } = "";
            public string Categoria { get; init; } = "";
            public string Precio { get; init; } = "";
            public string Stock { get; init; } = "";
            public ImageSource Imagen { get; init; } = ImageSource.FromFile("icono_consumo.png");
        }

        public MainPage(string nombreTrabajador)
        {
            InitializeComponent();
            _trabajador = nombreTrabajador;
            lblTrabajador.Text = $"Trabajador: {_trabajador}";
            RegistrarEventosSignalR();
            _ = CargarDatos();
            _ = InicializarConexiones();
            ConfigurarTimer();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _timer?.Start();
            _ = InicializarConexiones();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _timer?.Stop();
            _ = _signalRService.DetenerConexion();
        }

        private void RegistrarEventosSignalR()
        {
            if (_eventosSignalRRegistrados)
                return;

            _eventosSignalRRegistrados = true;
            _signalRService.MesaActualizada += ActualizarMesaDesdeSignalR;
            _signalRService.NuevaVentaRecibida += ActualizarVentaDesdeSignalR;
            _signalRService.EstadoConexionCambiado += estado =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    lblEstadoConexion.Text = estado switch
                    {
                        Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected => "Servidor: conectado",
                        Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connecting => "Servidor: conectando...",
                        Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Reconnecting => "Servidor: reconectando...",
                        _ => "Servidor: desconectado"
                    };
                    lblEstadoConexion.TextColor = estado switch
                    {
                        Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected => Color.FromArgb("#35C985"),
                        Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Reconnecting => Color.FromArgb("#F2C94C"),
                        Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connecting => Color.FromArgb("#F2C94C"),
                        _ => Color.FromArgb("#FF4C4C")
                    };
                });
                if (estado == Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected)
                    _ = CargarDatos();
            };
            _signalRService.ErrorConexion += mensaje =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    lblEstadoConexion.Text = $"Servidor: desconectado · {mensaje}");
            };
        }

        private async Task InicializarConexiones()
        {
            bool apiDisponible = await _signalRService.VerificarApi();
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (!apiDisponible)
                {
                    lblEstadoConexion.Text = "Servidor: API no disponible";
                    lblEstadoConexion.TextColor = Color.FromArgb("#FF4C4C");
                }
            });

            await _signalRService.IniciarConexion();
        }

        private void ActualizarMesaDesdeSignalR(int numeroMesa, string estado, string cliente)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (cvMesas.ItemsSource is not List<Mesa> mesas)
                {
                    await CargarDatos();
                    return;
                }

                var mesa = mesas.FirstOrDefault(m => m.NumeroMesa == numeroMesa);
                if (mesa == null)
                    return;

                bool ocupada = !string.Equals(estado, "LIBRE", StringComparison.OrdinalIgnoreCase);
                mesa.Ocupada = ocupada;
                mesa.Cliente = ocupada ? cliente : "";
                if (!ocupada)
                {
                    mesa.Modalidad = "";
                    mesa.HoraInicio = DateTime.MinValue;
                    mesa.MinutosFijos = 0;
                    mesa.ConsumoTotal = 0;
                }

                await _db.ActualizarMesa(mesa);
                lblMesasOcupadas.Text = $"{mesas.Count(m => m.Ocupada)} ocupadas";
                lblMesasLibres.Text = $"{mesas.Count(m => !m.Ocupada)} libres";
                if (_mesaSeleccionada?.NumeroMesa == numeroMesa)
                    MostrarDetalleMesa(mesa);
            });
        }

        private void ActualizarVentaDesdeSignalR(int numeroMesa, decimal total)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await ActualizarRecaudacion());
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
                lblTiempoModal.Text = _mesaSeleccionada.TiempoTranscurrido;
                var cobro = CalcularCobroMesa(_mesaSeleccionada, DateTime.Now - _mesaSeleccionada.HoraInicio);
                lblCobroMesaModal.Text = $"{cobro.Precio:0.00} Bs";
                lblSeccionTiempo.Text = cobro.Descripcion;
            }
        }

        private async Task CargarDatos()
        {
            var mesas = await _db.ObtenerMesas();
            _tarifasMesa = await _db.ObtenerTiposMesa();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                cvMesas.ItemsSource = mesas;
                lblMesasOcupadas.Text = $"{mesas.Count(m => m.Ocupada)} ocupadas";
                lblMesasLibres.Text = $"{mesas.Count(m => !m.Ocupada)} libres";
            });

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
            }
            cvMesas.SelectedItem = null;
        }

        private void MostrarDetalleMesa(Mesa mesa)
        {
            modalOverlay.IsVisible = true;
            lblModalMesa.Text = $"Mesa {mesa.NumeroMesa}";

            if (!mesa.Ocupada)
            {
                badgeModalMesa.BackgroundColor = (Color)Application.Current!.Resources["Surface"];
                lblEstadoModal.Text = "LIBRE";
                lblEstadoModal.TextColor = (Color)Application.Current.Resources["TextSecondary"];
                lblClienteModal.Text = "Sin cliente";
                lblTiempoModal.Text = "--:--:--";
                lblConsumoModal.Text = "0.00 Bs";
                lblCobroMesaModal.Text = "0.00 Bs";
                lblSeccionTiempo.Text = "Mesa libre";
                cvConsumosModal.ItemsSource = Array.Empty<ConsumoDetalle>();
                btnIniciarModal.IsVisible = true;
                btnAgregarModal.IsVisible = false;
                btnCobrarModal.IsVisible = false;
                return;
            }

            badgeModalMesa.BackgroundColor = (Color)Application.Current.Resources["SuccessSoft"];
            lblEstadoModal.Text = "OCUPADA";
            lblEstadoModal.TextColor = (Color)Application.Current.Resources["Success"];
            lblClienteModal.Text = string.IsNullOrWhiteSpace(mesa.Cliente) ? "Sin nombre" : mesa.Cliente;
            lblTiempoModal.Text = mesa.TiempoTranscurrido;
            lblConsumoModal.Text = $"{mesa.ConsumoTotal:0.00} Bs";
            var cobro = CalcularCobroMesa(mesa, DateTime.Now - mesa.HoraInicio);
            lblCobroMesaModal.Text = $"{cobro.Precio:0.00} Bs";
            lblSeccionTiempo.Text = cobro.Descripcion;
            _ = CargarDetalleConsumos(mesa.Id);
            btnIniciarModal.IsVisible = false;
            btnAgregarModal.IsVisible = true;
            btnCobrarModal.IsVisible = true;
        }

        private async Task CargarDetalleConsumos(int mesaId)
        {
            var consumos = await _db.ObtenerConsumosMesa(mesaId);
            var productos = await _db.ObtenerProductos();
            var detalle = consumos
                .GroupBy(c => c.ProductoId)
                .Select(grupo =>
                {
                    var producto = productos.FirstOrDefault(p => p.Id == grupo.Key);
                    int cantidad = grupo.Sum(c => c.Cantidad);
                    decimal precioUnitario = grupo.First().PrecioUnitario;
                    return new ConsumoDetalle
                    {
                        Nombre = grupo.First().NombreProducto,
                        Detalle = $"{cantidad} × {precioUnitario:0.00} Bs",
                        Subtotal = $"{cantidad * precioUnitario:0.00} Bs",
                        Imagen = MauiImageSourceHelper.Resolve(producto?.ImagenRuta, "icono_consumo.png")
                    };
                })
                .ToList();
            cvConsumosModal.ItemsSource = detalle;
        }

        private (decimal Precio, string Descripcion) CalcularCobroMesa(Mesa mesa, TimeSpan tiempo)
        {
            if (tiempo.TotalSeconds < 1)
                return (0m, "Aún no hay cobro por tiempo");

            var tipo = _tarifasMesa.FirstOrDefault(t => t.Id == mesa.TipoMesaId);
            decimal tarifaHora = tipo?.PrecioPorHora ?? 30m;
            decimal precioCuartoHora = tarifaHora / 4m;

            int minutosEfectivos = (int)Math.Floor(tiempo.TotalMinutes);
            if (mesa.Modalidad == "TIEMPO FIJO")
                minutosEfectivos = Math.Min(minutosEfectivos, mesa.MinutosFijos);
            if (minutosEfectivos < 1)
                return (0m, "Aún no hay cobro por tiempo");

            int bloquesCuartoHora = (int)Math.Ceiling(minutosEfectivos / 15.0);
            string descripcion = $"{bloquesCuartoHora * 15} minutos cobrables · {minutosEfectivos} min transcurridos";

            decimal precio = bloquesCuartoHora * precioCuartoHora;
            precio = Math.Ceiling(precio * 100m) / 100m;
            return (precio, descripcion);
        }

        private void MesasAreaSizeChanged(object sender, EventArgs e)
        {
            if (sender is not Border border) return;
            mesasLayout.Span = border.Width switch
            {
                >= 1100 => 5,
                >= 760 => 4,
                >= 470 => 3,
                _ => 2
            };
        }

        private void CerrarModal(object sender, EventArgs e)
        {
            modalOverlay.IsVisible = false;
            _mesaSeleccionada = null;
        }

        private async Task AbrirMesaSeleccionada()
        {
            if (_mesaSeleccionada != null)
                await IniciarMesa(_mesaSeleccionada);
        }

        private async void IniciarMesaDesdeModal(object sender, EventArgs e)
        {
            await AbrirMesaSeleccionada();
        }

        private async void AbrirMenuCuenta(object sender, EventArgs e)
        {
            string accion = await DisplayActionSheet("Opciones", "Cancelar", null,
                "Cerrar turno", "Cerrar sesión");

            if (accion == "Cerrar turno")
                CerrarTurno(sender, e);
            else if (accion == "Cerrar sesión")
                CerrarSesion(sender, e);
        }

        private async Task IniciarMesa(Mesa mesa)
        {
            string cliente = await DisplayPromptAsync("Nuevo Cliente", "Ingrese el nombre del cliente:", "OK", "Cancelar", "Cliente");
            if (string.IsNullOrWhiteSpace(cliente)) return;

            string modalidad = await DisplayActionSheet("Modalidad", "Cancelar", null, "TIEMPO LIBRE", "TIEMPO FIJO");

            if (modalidad == "TIEMPO FIJO")
            {
                int horas = await SolicitarEntero("Tiempo Fijo", "Ingrese las HORAS:", 0, true);
                if (horas < 0) return;

                int minutos = await SolicitarEntero("Tiempo Fijo", "Ingrese los MINUTOS:", 0, true);
                if (minutos < 0) return;

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
            await _signalRService.EnviarActualizacionMesa(mesa.NumeroMesa, "OCUPADA", mesa.Cliente);
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

            var categorias = await _db.ObtenerCategorias();
            var nombresCategorias = categorias
                .Where(c => c.Activo)
                .Select(c => c.Nombre)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (nombresCategorias.Count == 0)
            {
                await DisplayAlert("Sin categorías", "Primero registre categorías de productos.", "OK");
                return;
            }

            cvCategoriasConsumo.ItemsSource = nombresCategorias;
            cvCategoriasConsumo.SelectedItem = nombresCategorias.First();
            await MostrarProductosCategoria(nombresCategorias.First());
            consumoOverlay.IsVisible = true;
        }

        private async Task MostrarProductosCategoria(string categoria)
        {
            var productos = await _db.ObtenerProductos();
            var filas = productos
                .Where(p => p.Stock > 0 &&
                            string.Equals(p.CategoriaNombre, categoria, StringComparison.OrdinalIgnoreCase))
                .Select(p => new ProductoConsumoRow
                {
                    Producto = p,
                    Nombre = p.Nombre,
                    Categoria = p.CategoriaNombre,
                    Precio = $"{p.Precio:0.00} Bs",
                    Stock = $"Stock {p.Stock}",
                    Imagen = MauiImageSourceHelper.Resolve(p.ImagenRuta, "icono_consumo.png")
                })
                .ToList();

            cvProductosConsumo.ItemsSource = filas;
        }

        private async void CategoriaConsumoSeleccionada(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not string categoria) return;
            await MostrarProductosCategoria(categoria);
        }

        private async void ProductoConsumoSeleccionado(object sender, SelectionChangedEventArgs e)
        {
            cvProductosConsumo.SelectedItem = null;
            if (e.CurrentSelection.FirstOrDefault() is not ProductoConsumoRow fila) return;

            var productoSeleccionado = fila.Producto;
            if (productoSeleccionado.Stock <= 0)
            {
                await DisplayAlert("Inventario agotado", $"No quedan unidades de {productoSeleccionado.Nombre}.", "OK");
                return;
            }

            int cantidad = await SolicitarCantidad($"¿Cuántas unidades de {productoSeleccionado.Nombre} desea agregar?");
            if (cantidad <= 0)
                return;
            if (cantidad > productoSeleccionado.Stock)
            {
                await DisplayAlert("Stock insuficiente",
                    $"Solo quedan {productoSeleccionado.Stock} unidades de {productoSeleccionado.Nombre}.",
                    "OK");
                return;
            }

            decimal subtotal = productoSeleccionado.Precio * cantidad;
            _mesaSeleccionada.ConsumoTotal += subtotal;
            productoSeleccionado.Stock -= cantidad;
            await _db.GuardarProducto(productoSeleccionado);
            await _db.ActualizarMesa(_mesaSeleccionada);
            await _signalRService.EnviarActualizacionMesa(
                _mesaSeleccionada.NumeroMesa,
                "OCUPADA",
                _mesaSeleccionada.Cliente);
            await _db.AgregarConsumoMesa(new ConsumoMesa
            {
                MesaId = _mesaSeleccionada.Id,
                ProductoId = productoSeleccionado.Id,
                NombreProducto = productoSeleccionado.Nombre,
                Cantidad = cantidad,
                PrecioUnitario = productoSeleccionado.Precio,
                PrecioCompraUnitario = productoSeleccionado.PrecioCompraUnidad
            });
            await _db.RegistrarAuditoria(new Modelo.RegistroAuditoria
            {
                Fecha = DateTime.Now,
                Usuario = _trabajador,
                Rol = "TRABAJADOR",
                Accion = "CONSUMO DE PRODUCTO",
                Entidad = "Producto",
                Detalle = $"{cantidad} × {productoSeleccionado.Nombre} · Mesa {_mesaSeleccionada.NumeroMesa} · Stock restante {productoSeleccionado.Stock}"
            });

            await DisplayAlert("Consumo",
                $"Se agregaron {cantidad} unidades de {productoSeleccionado.Nombre}.\nSubtotal: {subtotal:0.00} Bs",
                "OK");
            await CargarDatos();
            MostrarDetalleMesa(_mesaSeleccionada);
        }

        private async Task<int> SolicitarCantidad(string mensaje)
        {
            while (true)
            {
                string texto = await DisplayPromptAsync(
                    "Cantidad",
                    mensaje,
                    "Agregar",
                    "Cancelar",
                    "1",
                    -1,
                    Keyboard.Numeric);

                if (texto == null)
                    return 0;
                if (int.TryParse(texto, out int cantidad) && cantidad > 0)
                    return cantidad;

                await DisplayAlert("Número inválido", "Ingrese números enteros mayores que cero.", "OK");
            }
        }

        private async Task<int> SolicitarEntero(string titulo, string mensaje, int valorInicial, bool permitirCero)
        {
            while (true)
            {
                string texto = await DisplayPromptAsync(
                    titulo,
                    mensaje,
                    "Aceptar",
                    "Cancelar",
                    valorInicial.ToString(),
                    -1,
                    Keyboard.Numeric);

                if (texto == null)
                    return -1;
                if (int.TryParse(texto, out int valor) &&
                    (permitirCero ? valor >= 0 : valor > 0))
                    return valor;

                await DisplayAlert(
                    "Número inválido",
                    permitirCero
                        ? "Ingrese solamente números y un valor igual o mayor que cero."
                        : "Ingrese solamente números mayores que cero.",
                    "OK");
            }
        }

        private void CerrarSelectorConsumo(object sender, EventArgs e)
        {
            consumoOverlay.IsVisible = false;
            cvCategoriasConsumo.SelectedItem = null;
            cvProductosConsumo.SelectedItem = null;
        }

        private async void CobrarMesaDetalle(object sender, EventArgs e)
        {
            if (_mesaSeleccionada == null || !_mesaSeleccionada.Ocupada)
            {
                await DisplayAlert("Aviso", "Seleccione una mesa ocupada para cobrar.", "OK");
                return;
            }

            TimeSpan tiempoTranscurrido = DateTime.Now - _mesaSeleccionada.HoraInicio;
            var cobroMesa = CalcularCobroMesa(_mesaSeleccionada, tiempoTranscurrido);
            decimal precioMesa = cobroMesa.Precio;
            string duracion = cobroMesa.Descripcion;

            decimal consumoMesa = _mesaSeleccionada.ConsumoTotal;
            decimal totalGeneral = precioMesa + consumoMesa;

            var venta = new Venta
            {
                NumeroMesa = _mesaSeleccionada.NumeroMesa,
                Cliente = _mesaSeleccionada.Cliente,
                Fecha = DateTime.Now,
                Duracion = duracion,
                TotalMesa = precioMesa,
                TotalConsumo = consumoMesa,
                TotalGeneral = totalGeneral,
                Trabajador = _trabajador,
                Entregado = false
            };

            await _db.GuardarVenta(venta);

            var consumosMesa = await _db.ObtenerConsumosMesa(_mesaSeleccionada.Id);
            foreach (var consumoMesaPendiente in consumosMesa)
            {
                await _db.InsertarVentaProducto(new VentaProducto
                {
                    VentaId = venta.Id,
                    ProductoId = consumoMesaPendiente.ProductoId,
                    NombreProducto = consumoMesaPendiente.NombreProducto,
                    Cantidad = consumoMesaPendiente.Cantidad,
                    PrecioUnitario = consumoMesaPendiente.PrecioUnitario,
                    PrecioCompraUnitario = consumoMesaPendiente.PrecioCompraUnitario
                });
            }
            await _db.LimpiarConsumosMesa(_mesaSeleccionada.Id);

            _mesaSeleccionada.Ocupada = false;
            _mesaSeleccionada.Cliente = "";
            _mesaSeleccionada.Modalidad = "";
            _mesaSeleccionada.ConsumoTotal = 0;
            _mesaSeleccionada.HoraInicio = DateTime.MinValue;
            _mesaSeleccionada.MinutosFijos = 0;
            _mesaSeleccionada.TiempoTranscurrido = "";
            await _db.ActualizarMesa(_mesaSeleccionada);
            await _signalRService.EnviarActualizacionMesa(
                _mesaSeleccionada.NumeroMesa,
                "LIBRE",
                "");
            await _signalRService.EnviarNuevaVenta(
                _mesaSeleccionada.NumeroMesa,
                totalGeneral);

            await DisplayAlert("Cobro Exitoso",
                $"Mesa: {_mesaSeleccionada.NumeroMesa}\n" +
                $"Cliente: {venta.Cliente}\n" +
                $"Tiempo: {duracion}\n" +
                $"Mesa: {precioMesa:0.00} Bs\n" +
                $"Consumo: {consumoMesa:0.00} Bs\n" +
                $"TOTAL: {totalGeneral:0.00} Bs", "OK");

            _mesaSeleccionada = null;
            modalOverlay.IsVisible = false;

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
                DateTime fechaInicio = await _db.ObtenerInicioUltimoTurno(_trabajador);
                await _db.CerrarCaja(_trabajador);
                await _db.RegistrarCierre(new CierreCaja
                {
                    Trabajador = _trabajador,
                    FechaInicio = fechaInicio,
                    Fecha = DateTime.Now,
                    TotalMesas = totalMesas,
                    TotalConsumo = totalConsumo,
                    TotalGeneral = totalGeneral,
                    CantidadVentas = ventas.Count,
                    IdsVentas = string.Join(",", ventas.Select(v => v.Id))
                });
                await _db.RegistrarAuditoria(new Modelo.RegistroAuditoria
                {
                    Fecha = DateTime.Now,
                    Usuario = _trabajador,
                    Rol = "TRABAJADOR",
                    Accion = "CIERRE DE CAJA",
                    Entidad = "Caja",
                    Detalle = $"{ventas.Count} ventas · Mesas {totalMesas:0.00} Bs · Consumo {totalConsumo:0.00} Bs · Total {totalGeneral:0.00} Bs"
                });
                await DisplayAlert("Caja Cerrada", "El turno ha sido cerrado y la caja entregada al administrador.", "OK");
                await CargarDatos();
            }
        }

        private async void CerrarSesion(object sender, EventArgs e)
        {
            await _db.RegistrarAuditoria(new Modelo.RegistroAuditoria
            {
                Fecha = DateTime.Now,
                Usuario = _trabajador,
                Rol = "TRABAJADOR",
                Accion = "CIERRE DE SESIÓN",
                Entidad = "Sesión",
                Detalle = "El trabajador cerró su sesión"
            });
            await Navigation.PopToRootAsync();
        }
    }
}