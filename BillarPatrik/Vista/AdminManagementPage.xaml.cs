using BillarPatrik.Data;
using BillarPatrik.Modelo;

namespace BillarPatrik.Vista
{
    public partial class AdminManagementPage : ContentPage
    {
        private readonly ConexionDB _db = new ConexionDB();
        private string _modulo;
        private List<AdminRow> _filas = new();

        private sealed class AdminRow
        {
            public object? Entidad { get; init; }
            public string Titulo { get; init; } = "";
            public string Subtitulo { get; init; } = "";
            public string Estado { get; init; } = "";
            public Color FondoEstado { get; init; } = Colors.Transparent;
            public Color ColorEstado { get; init; } = Colors.Gray;
            public ImageSource? Imagen { get; init; }
            public bool TieneImagen => Imagen != null;
        }

        public AdminManagementPage(string modulo)
        {
            InitializeComponent();
            _modulo = modulo;
            _ = CambiarModulo(modulo);
        }

        private async Task CambiarModulo(string modulo)
        {
            _modulo = modulo;
            txtBuscar.Text = "";
            RestablecerBotones();
            btnAgregar.IsVisible = modulo is "mesas" or "tipos" or "trabajadores" or "categorias" or "productos";
            btnExportar.IsVisible = modulo is "caja" or "auditoria";

            switch (modulo)
            {
                case "tipos":
                    lblTitulo.Text = "Tipos de mesa";
                    lblSubtitulo.Text = "Normal, privada y tarifas";
                    ActivarBoton(btnTipos);
                    await CargarTipos();
                    break;
                case "trabajadores":
                    lblTitulo.Text = "Trabajadores";
                    lblSubtitulo.Text = "Usuarios con acceso al panel";
                    ActivarBoton(btnTrabajadores);
                    await CargarTrabajadores();
                    break;
                case "categorias":
                    lblTitulo.Text = "Categorías";
                    lblSubtitulo.Text = "Clasificación de productos";
                    ActivarBoton(btnCategorias);
                    await CargarCategorias();
                    break;
                case "productos":
                    lblTitulo.Text = "Productos";
                    lblSubtitulo.Text = "Inventario, precios e imágenes";
                    ActivarBoton(btnProductos);
                    await CargarProductos();
                    break;
                case "caja":
                    lblTitulo.Text = "Registros de caja";
                    lblSubtitulo.Text = "Inicio y cierre de turno, movimiento y ventas";
                    ActivarBoton(btnCaja);
                    await CargarCaja();
                    break;
                case "contabilidad":
                    lblTitulo.Text = "Contabilidad";
                    lblSubtitulo.Text = "Ingresos, costos y rentabilidad";
                    ActivarBoton(btnContabilidad);
                    await CargarContabilidad();
                    break;
                case "auditoria":
                    lblTitulo.Text = "Registros de caja";
                    lblSubtitulo.Text = "Sesiones, cierres y operaciones detalladas";
                    ActivarBoton(btnAuditoria);
                    await CargarAuditoria();
                    break;
                default:
                    lblTitulo.Text = "Gestión de mesas";
                    lblSubtitulo.Text = "Mesas y asignación de tipos";
                    ActivarBoton(btnMesas);
                    await CargarMesas();
                    break;
            }
        }

        private void RestablecerBotones()
        {
            var botones = new[] { btnMesas, btnTipos, btnTrabajadores, btnCategorias, btnProductos, btnCaja, btnContabilidad, btnAuditoria };
            foreach (var boton in botones)
            {
                boton.BackgroundColor = Color.FromArgb("#1A1A1A");
                boton.TextColor = Color.FromArgb("#F5F5F5");
                boton.BorderColor = Color.FromArgb("#333333");
                boton.BorderWidth = 1;
            }
        }

        private void ActivarBoton(Button boton)
        {
            boton.BackgroundColor = Color.FromArgb("#0D2B1F");
            boton.TextColor = Color.FromArgb("#F5F5F5");
            boton.BorderColor = Color.FromArgb("#1B4D3E");
            boton.BorderWidth = 1;
        }

        private async Task CargarMesas()
        {
            var mesas = await _db.ObtenerMesas();
            var tipos = await _db.ObtenerTiposMesa();
            _filas = mesas.Select(m =>
            {
                var tipo = tipos.FirstOrDefault(t => t.Id == m.TipoMesaId);
                return new AdminRow
                {
                    Entidad = m,
                    Titulo = $"Mesa {m.NumeroMesa}",
                    Subtitulo = tipo?.Nombre ?? "Sin tipo",
                    Estado = m.Ocupada ? "OCUPADA" : "LIBRE",
                    FondoEstado = m.Ocupada ? (Color)Application.Current!.Resources["DangerSoft"] : Color.FromArgb("#1E2A22"),
                    ColorEstado = m.Ocupada ? (Color)Application.Current.Resources["Danger"] : (Color)Application.Current.Resources["Success"],
                    Imagen = ImageSource.FromFile("mesa.png")
                };
            }).ToList();
            AplicarBusqueda();
        }

        private async Task CargarTipos()
        {
            var tipos = await _db.ObtenerTiposMesa();
            _filas = tipos.Select(t => new AdminRow
            {
                Entidad = t,
                Titulo = t.Nombre,
                Subtitulo = $"Hora {t.PrecioPorHora:0.00} Bs · 30 min {t.PrecioPorHora / 2m:0.00} Bs · 15 min {t.PrecioPorHora / 4m:0.00} Bs",
                Estado = t.Activo ? "ACTIVO" : "INACTIVO",
                FondoEstado = t.Activo ? (Color)Application.Current!.Resources["SuccessSoft"] : Color.FromArgb("#FEE2E2"),
                ColorEstado = t.Activo ? (Color)Application.Current.Resources["Success"] : (Color)Application.Current.Resources["Danger"],
                Imagen = ImageSource.FromFile("mesa.png")
            }).ToList();
            AplicarBusqueda();
        }

        private async Task CargarTrabajadores()
        {
            var usuarios = await _db.ObtenerTrabajadores();
            _filas = usuarios.Select(u => new AdminRow
            {
                Entidad = u,
                Titulo = u.Nombre,
                Subtitulo = $"Usuario: {u.UsuarioLogin}",
                Estado = "TRABAJADOR",
                FondoEstado = (Color)Application.Current!.Resources["SuccessSoft"],
                ColorEstado = (Color)Application.Current.Resources["Success"],
                Imagen = null
            }).ToList();
            AplicarBusqueda();
        }

        private async Task CargarCategorias()
        {
            var categorias = await _db.ObtenerCategorias();
            _filas = categorias.Select(c => new AdminRow
            {
                Entidad = c,
                Titulo = c.Nombre,
                Subtitulo = "Categoría de productos",
                Estado = c.Activo ? "ACTIVA" : "INACTIVA",
                FondoEstado = c.Activo ? (Color)Application.Current!.Resources["SuccessSoft"] : Color.FromArgb("#FEE2E2"),
                ColorEstado = c.Activo ? (Color)Application.Current.Resources["Success"] : (Color)Application.Current.Resources["Danger"],
                Imagen = null
            }).ToList();
            AplicarBusqueda();
        }

        private async Task CargarProductos()
        {
            var productos = await _db.ObtenerProductos();
            _filas = productos.Select(p => new AdminRow
            {
                Entidad = p,
                Titulo = p.Nombre,
                Subtitulo = $"{p.CategoriaNombre} · Stock {p.Stock} · Venta {p.PrecioVentaUnidad:0.00} Bs",
                Estado = $"{p.GananciaUnidad:0.00} Bs GAN.",
                FondoEstado = (Color)Application.Current!.Resources["BrandGoldLight"],
                ColorEstado = (Color)Application.Current.Resources["BrandNavy"],
                Imagen = MauiImageSourceHelper.Resolve(p.ImagenRuta, "icono_consumo.png")
            }).ToList();
            AplicarBusqueda();
        }

        private async Task CargarCaja()
        {
            var cierres = await _db.ObtenerCierresCaja();
            var ventas = await _db.ObtenerTodasLasVentas();
            var pendientes = ventas.Where(v => !v.Entregado).ToList();
            _filas = cierres.Select(c => new AdminRow
            {
                Entidad = c,
                Titulo = c.Trabajador,
                Subtitulo = $"{c.Fecha:dd/MM/yyyy HH:mm} · {c.CantidadVentas} ventas · Mesas {c.TotalMesas:0.00} Bs",
                Estado = $"{c.TotalGeneral:0.00} Bs",
                FondoEstado = (Color)Application.Current!.Resources["SuccessSoft"],
                ColorEstado = (Color)Application.Current.Resources["Success"]
            }).ToList();

            if (pendientes.Count > 0)
            {
                _filas.Insert(0, new AdminRow
                {
                    Titulo = "Caja pendiente",
                    Subtitulo = $"{pendientes.Count} ventas aún no entregadas",
                    Estado = $"{pendientes.Sum(v => v.TotalGeneral):0.00} Bs",
                    FondoEstado = (Color)Application.Current.Resources["BrandGoldLight"],
                    ColorEstado = (Color)Application.Current.Resources["BrandNavy"]
                });
            }
            AplicarBusqueda();
        }

        private async Task CargarAuditoria()
        {
            var registros = await _db.ObtenerAuditoria();
            var cierres = await _db.ObtenerCierresCaja();
            _filas = registros.Select(r => new AdminRow
            {
                Entidad = r,
                Titulo = r.Accion,
                Subtitulo = $"{r.Fecha:dd/MM/yyyy HH:mm:ss} · {r.Usuario} · {r.Entidad}",
                Estado = r.Rol == "" ? "SISTEMA" : r.Rol,
                FondoEstado = r.Accion.Contains("FALLIDO") ? Color.FromArgb("#4A1D23") : Color.FromArgb("#1E2A22"),
                ColorEstado = r.Accion.Contains("FALLIDO") ? Color.FromArgb("#FF4C4C") : Color.FromArgb("#35C985")
            }).ToList();

            foreach (var cierre in cierres.OrderByDescending(c => c.Fecha).Take(20))
            {
                _filas.Insert(0, new AdminRow
                {
                    Entidad = cierre,
                    Titulo = "CIERRE DE CAJA",
                    Subtitulo = $"{cierre.Fecha:dd/MM/yyyy HH:mm:ss} · {cierre.Trabajador} · {cierre.CantidadVentas} ventas",
                    Estado = $"{cierre.TotalGeneral:0.00} Bs",
                    FondoEstado = Color.FromArgb("#1E2A22"),
                    ColorEstado = Color.FromArgb("#35C985")
                });
            }
            AplicarBusqueda();
        }

        private async Task CargarContabilidad()
        {
            var ventas = await _db.ObtenerTodasLasVentas();
            var ventasProducto = await _db.ObtenerVentasProducto();
            var compras = await _db.ObtenerComprasProducto();
            var ahora = DateTime.Now;
            var hoy = ventas.Where(v => v.Fecha.Date == ahora.Date).ToList();
            var semana = ventas.Where(v => v.Fecha >= ahora.Date.AddDays(-7)).ToList();
            var mes = ventas.Where(v => v.Fecha >= ahora.Date.AddDays(-30)).ToList();
            decimal gananciaProductos = ventasProducto.Sum(v => v.Ganancia);
            decimal costoCompras = compras.Sum(c => c.CostoTotal);
            decimal ingresoMesas = mes.Sum(v => v.TotalMesa);
            decimal ingresoProductos = mes.Sum(v => v.TotalConsumo);

            _filas = new List<AdminRow>
            {
                new() { Titulo = "Ganancia hoy", Subtitulo = $"Ingresos productos y mesas", Estado = $"{hoy.Sum(v => v.TotalGeneral):0.00} Bs", FondoEstado = (Color)Application.Current!.Resources["SuccessSoft"], ColorEstado = (Color)Application.Current.Resources["Success"] },
                new() { Titulo = "Ingreso 7 días", Subtitulo = $"{semana.Count} ventas registradas", Estado = $"{semana.Sum(v => v.TotalGeneral):0.00} Bs", FondoEstado = (Color)Application.Current.Resources["BrandGoldLight"], ColorEstado = (Color)Application.Current.Resources["BrandNavy"] },
                new() { Titulo = "Ingreso 30 días", Subtitulo = $"{mes.Count} ventas registradas", Estado = $"{mes.Sum(v => v.TotalGeneral):0.00} Bs", FondoEstado = (Color)Application.Current.Resources["SuccessSoft"], ColorEstado = (Color)Application.Current.Resources["Success"] },
                new() { Titulo = "Ganancia estimada productos", Subtitulo = "Venta menos costo unitario", Estado = $"{gananciaProductos:0.00} Bs", FondoEstado = (Color)Application.Current.Resources["BrandGoldLight"], ColorEstado = (Color)Application.Current.Resources["BrandNavy"] },
                new() { Titulo = "Ventas de mesas", Subtitulo = "Ingresos de los últimos 30 días", Estado = $"{ingresoMesas:0.00} Bs", FondoEstado = (Color)Application.Current.Resources["Surface"], ColorEstado = (Color)Application.Current.Resources["BrandBlue"] },
                new() { Titulo = "Ventas de productos", Subtitulo = "Consumos de los últimos 30 días", Estado = $"{ingresoProductos:0.00} Bs", FondoEstado = (Color)Application.Current.Resources["Surface"], ColorEstado = (Color)Application.Current.Resources["BrandBlue"] },
                new() { Titulo = "Compras registradas", Subtitulo = "Costo total de productos adquiridos", Estado = $"{costoCompras:0.00} Bs", FondoEstado = (Color)Application.Current.Resources["BrandGoldLight"], ColorEstado = (Color)Application.Current.Resources["BrandNavy"] }
            };

            var ranking = ventasProducto
                .GroupBy(v => v.NombreProducto)
                .Select(g => new { Nombre = g.Key, Cantidad = g.Sum(v => v.Cantidad), Ganancia = g.Sum(v => v.Ganancia) })
                .OrderByDescending(g => g.Cantidad)
                .Take(10)
                .ToList();

            foreach (var producto in ranking)
            {
                _filas.Add(new AdminRow
                {
                    Titulo = producto.Nombre,
                    Subtitulo = $"Producto más vendido · Ganancia estimada {producto.Ganancia:0.00} Bs",
                    Estado = $"{producto.Cantidad} UN.",
                    FondoEstado = (Color)Application.Current.Resources["Surface"],
                    ColorEstado = (Color)Application.Current.Resources["BrandBlue"]
                });
            }

            foreach (var compra in compras.Take(10))
            {
                _filas.Add(new AdminRow
                {
                    Titulo = compra.NombreProducto,
                    Subtitulo = $"{compra.Fecha:dd/MM/yyyy} · {compra.Cajas} cajas × {compra.UnidadesPorCaja} unidades",
                    Estado = $"{compra.CostoTotal:0.00} Bs",
                    FondoEstado = (Color)Application.Current.Resources["BrandGoldLight"],
                    ColorEstado = (Color)Application.Current.Resources["BrandNavy"]
                });
            }
            AplicarBusqueda();
        }

        private void AplicarBusqueda()
        {
            string texto = txtBuscar.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(texto))
                cvDatos.ItemsSource = _filas;
            else
                cvDatos.ItemsSource = _filas.Where(f =>
                    f.Titulo.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    f.Subtitulo.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void BuscarTexto(object sender, TextChangedEventArgs e) => AplicarBusqueda();

        private async void Volver(object sender, EventArgs e) => await Navigation.PopAsync();
        private async void CambiarMesas(object sender, EventArgs e) => await CambiarModulo("mesas");
        private async void CambiarTipos(object sender, EventArgs e) => await CambiarModulo("tipos");
        private async void CambiarTrabajadores(object sender, EventArgs e) => await CambiarModulo("trabajadores");
        private async void CambiarCategorias(object sender, EventArgs e) => await CambiarModulo("categorias");
        private async void CambiarProductos(object sender, EventArgs e) => await CambiarModulo("productos");
        private async void CambiarCaja(object sender, EventArgs e) => await CambiarModulo("caja");
        private async void CambiarContabilidad(object sender, EventArgs e) => await CambiarModulo("contabilidad");
        private async void CambiarAuditoria(object sender, EventArgs e) => await CambiarModulo("auditoria");

        private async void ExportarPdfGeneral(object sender, EventArgs e)
        {
            var cierres = await _db.ObtenerCierresCaja();
            await ExportarPdf(cierres);
        }

        private async Task MostrarDetalleCierre(CierreCaja cierre)
        {
            var ventas = await _db.ObtenerVentasDeCierre(cierre);
            var ventaProductos = await _db.ObtenerVentasProducto();
            var ventasIds = ventas.Select(v => v.Id).ToHashSet();
            var productos = ventaProductos
                .Where(v => ventasIds.Contains(v.VentaId))
                .GroupBy(v => v.NombreProducto)
                .Select(g => $"{g.Key}: {g.Sum(v => v.Cantidad)} un. · {g.Sum(v => v.Subtotal):0.00} Bs")
                .ToList();

            var lineas = new System.Text.StringBuilder();
            lineas.AppendLine($"TRABAJADOR: {cierre.Trabajador}");
            lineas.AppendLine($"INICIO: {cierre.FechaInicio:dd/MM/yyyy HH:mm:ss}");
            lineas.AppendLine($"CIERRE: {cierre.Fecha:dd/MM/yyyy HH:mm:ss}");
            lineas.AppendLine($"MESAS: {cierre.TotalMesas:0.00} Bs");
            lineas.AppendLine($"PRODUCTOS: {cierre.TotalConsumo:0.00} Bs");
            lineas.AppendLine($"TOTAL ENTREGADO: {cierre.TotalGeneral:0.00} Bs");
            lineas.AppendLine("");
            lineas.AppendLine("VENTAS DE MESAS:");
            foreach (var venta in ventas)
            {
                lineas.AppendLine($"Mesa {venta.NumeroMesa} · {venta.Cliente} · {venta.Duracion} · Mesa {venta.TotalMesa:0.00} Bs · Consumo {venta.TotalConsumo:0.00} Bs · Total {venta.TotalGeneral:0.00} Bs");
            }
            lineas.AppendLine("");
            lineas.AppendLine("PRODUCTOS VENDIDOS:");
            if (productos.Count == 0)
                lineas.AppendLine("Sin productos");
            else
                productos.ForEach(p => lineas.AppendLine(p));

            await DisplayAlert("Detalle del turno", lineas.ToString(), "Cerrar");
        }

        private async Task ExportarPdf(List<CierreCaja> cierres)
        {
            if (cierres.Count == 0)
            {
                await DisplayAlert("Sin registros", "No hay cierres para exportar.", "OK");
                return;
            }

            var lineas = new List<string>
            {
                "BILLAR PATRIK",
                "REGISTROS DE CAJA Y MOVIMIENTO DE TURNOS",
                $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
                ""
            };
            var todosProductos = await _db.ObtenerVentasProducto();

            foreach (var cierre in cierres)
            {
                var ventas = await _db.ObtenerVentasDeCierre(cierre);
                var idsVentas = ventas.Select(v => v.Id).ToHashSet();
                lineas.Add($"TRABAJADOR: {cierre.Trabajador}");
                lineas.Add($"INICIO: {cierre.FechaInicio:dd/MM/yyyy HH:mm:ss}");
                lineas.Add($"CIERRE: {cierre.Fecha:dd/MM/yyyy HH:mm:ss}");
                lineas.Add($"Ventas: {ventas.Count}");
                lineas.Add($"Mesas: {cierre.TotalMesas:0.00} Bs");
                lineas.Add($"Productos: {cierre.TotalConsumo:0.00} Bs");
                lineas.Add($"TOTAL: {cierre.TotalGeneral:0.00} Bs");
                lineas.Add("Detalle de mesas:");
                foreach (var venta in ventas)
                {
                    lineas.Add($"Mesa {venta.NumeroMesa} - {venta.Cliente} - {venta.Duracion}");
                    lineas.Add($"Mesa {venta.TotalMesa:0.00} Bs; Consumo {venta.TotalConsumo:0.00} Bs; Total {venta.TotalGeneral:0.00} Bs");
                }

                lineas.Add("Productos vendidos:");
                var productos = todosProductos
                    .Where(v => idsVentas.Contains(v.VentaId))
                    .GroupBy(v => v.NombreProducto)
                    .Select(g => $"{g.Key}: {g.Sum(v => v.Cantidad)} un. - {g.Sum(v => v.Subtotal):0.00} Bs")
                    .ToList();
                if (productos.Count == 0)
                    lineas.Add("Sin productos");
                else
                    lineas.AddRange(productos);
                lineas.Add("");
            }

            string archivo = Path.Combine(FileSystem.CacheDirectory,
                $"RegistrosCaja_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            await File.WriteAllBytesAsync(archivo, CrearPdfBasico(lineas));
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Exportar registros de caja",
                File = new ShareFile(archivo)
            });
        }

        private static byte[] CrearPdfBasico(List<string> lineas)
        {
            const int maxLineasPagina = 45;
            var paginas = new List<string>();
            for (int i = 0; i < lineas.Count; i += maxLineasPagina)
            {
                var pagina = lineas.Skip(i).Take(maxLineasPagina).Select(NormalizarPdf).ToList();
                paginas.Add(string.Join("\n", pagina.Select(l =>
                    $"({l.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)")}) Tj\n0 -14 Td")));
            }
            if (paginas.Count == 0)
                paginas.Add("(Sin registros) Tj");

            var salida = new MemoryStream();
            var writer = System.Text.Encoding.ASCII.GetBytes("%PDF-1.4\n");
            salida.Write(writer, 0, writer.Length);
            var offsets = new List<long> { 0 };
            var kids = string.Join(" ", Enumerable.Range(0, paginas.Count)
                .Select(i => $"{4 + i * 2} 0 R"));
            string[] objsIniciales =
            {
                "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n",
                $"2 0 obj\n<< /Type /Pages /Kids [{kids}] /Count {paginas.Count} >>\nendobj\n",
                "3 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n"
            };

            foreach (var obj in objsIniciales)
            {
                offsets.Add(salida.Length);
                var bytes = System.Text.Encoding.ASCII.GetBytes(obj);
                salida.Write(bytes, 0, bytes.Length);
            }

            for (int i = 0; i < paginas.Count; i++)
            {
                int pageObject = 4 + i * 2;
                int contentObject = pageObject + 1;
                offsets.Add(salida.Length);
                var page = System.Text.Encoding.ASCII.GetBytes(
                    $"{pageObject} 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentObject} 0 R >>\nendobj\n");
                salida.Write(page, 0, page.Length);

                string stream = "BT\n/F1 10 Tf\n50 760 Td\n" + paginas[i] + "\nET";
                offsets.Add(salida.Length);
                var content = System.Text.Encoding.ASCII.GetBytes(
                    $"{contentObject} 0 obj\n<< /Length {System.Text.Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream\nendobj\n");
                salida.Write(content, 0, content.Length);
            }

            long xref = salida.Length;
            int objectCount = 3 + paginas.Count * 2;
            var xrefText = $"xref\n0 {objectCount + 1}\n0000000000 65535 f \n" +
                           string.Join("", offsets.Skip(1).Select(o => $"{o:0000000000} 00000 n \n"));
            var trailer = $"trailer\n<< /Size {objectCount + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF";
            var xrefBytes = System.Text.Encoding.ASCII.GetBytes(xrefText);
            var trailerBytes = System.Text.Encoding.ASCII.GetBytes(trailer);
            salida.Write(xrefBytes, 0, xrefBytes.Length);
            salida.Write(trailerBytes, 0, trailerBytes.Length);
            return salida.ToArray();
        }

        private static string NormalizarPdf(string texto)
        {
            return texto
                .Replace("á", "a").Replace("Á", "A")
                .Replace("é", "e").Replace("É", "E")
                .Replace("í", "i").Replace("Í", "I")
                .Replace("ó", "o").Replace("Ó", "O")
                .Replace("ú", "u").Replace("Ú", "U")
                .Replace("ñ", "n").Replace("Ñ", "N")
                .Replace("¿", "").Replace("¡", "");
        }

        private async void CambiarFiltro(object sender, EventArgs e)
        {
            if (_modulo == "caja")
            {
                string opcion = await DisplayActionSheet("Filtrar caja", "Cancelar", null, "Todos", "Hoy", "Últimos 7 días", "Últimos 30 días");
                btnFiltro.Text = opcion == "Cancelar" || opcion == null ? "TODOS" : opcion.ToUpperInvariant();
                await CargarCaja();
                if (opcion is "Hoy" or "Últimos 7 días" or "Últimos 30 días")
                {
                    DateTime limite = opcion == "Hoy" ? DateTime.Now.Date :
                        opcion == "Últimos 7 días" ? DateTime.Now.AddDays(-7) : DateTime.Now.AddDays(-30);
                    _filas = _filas.Where(f => f.Entidad is CierreCaja c && c.Fecha >= limite).ToList();
                    AplicarBusqueda();
                }
            }
            else
            {
                await DisplayAlert("Filtro", "Este módulo permite buscar por nombre con el campo superior.", "OK");
            }
        }

        private async void Agregar(object sender, EventArgs e)
        {
            if (_modulo == "mesas") await AgregarMesa();
            else if (_modulo == "tipos") await AgregarTipo();
            else if (_modulo == "trabajadores") await AgregarTrabajador();
            else if (_modulo == "categorias") await AgregarCategoria();
            else if (_modulo == "productos") await AgregarProducto();
        }

        private async Task<int> SolicitarEntero(string titulo, string mensaje, string valorInicial, bool mayorQueCero)
        {
            while (true)
            {
                string texto = await DisplayPromptAsync(
                    titulo,
                    mensaje,
                    "Aceptar",
                    "Cancelar",
                    valorInicial,
                    -1,
                    Keyboard.Numeric);
                if (texto == null)
                    return 0;
                if (int.TryParse(texto, out int valor) &&
                    (mayorQueCero ? valor > 0 : valor >= 0))
                    return valor;

                await DisplayAlert(
                    "Número inválido",
                    mayorQueCero
                        ? "Ingrese solamente números enteros mayores que cero."
                        : "Ingrese solamente números enteros.",
                    "OK");
            }
        }

        private async Task<decimal> SolicitarDecimal(string titulo, string mensaje, string valorInicial, bool permitirCero)
        {
            while (true)
            {
                string texto = await DisplayPromptAsync(
                    titulo,
                    mensaje,
                    "Aceptar",
                    "Cancelar",
                    valorInicial,
                    -1,
                    Keyboard.Numeric);
                if (texto == null)
                    return -1m;
                texto = texto.Replace(',', '.');
                if (decimal.TryParse(texto,
                        System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal valor) &&
                    (permitirCero ? valor > 0 : valor >= 0))
                    return valor;

                await DisplayAlert(
                    "Número inválido",
                    permitirCero
                        ? "Ingrese solamente números mayores que cero."
                        : "Ingrese solamente números iguales o mayores que cero.",
                    "OK");
            }
        }

        private async Task AgregarTipo()
        {
            string nombre = await DisplayPromptAsync("Nuevo tipo", "Nombre del tipo:", "Guardar", "Cancelar", "Mesa normal");
            if (string.IsNullOrWhiteSpace(nombre)) return;
            decimal precio = await SolicitarDecimal("Tarifa", "Precio por hora:", "30", true);
            if (precio < 0) return;
            await _db.GuardarTipoMesa(new TipoMesa { Nombre = nombre.Trim(), PrecioPorHora = precio, Activo = true });
            await CargarTipos();
        }

        private async Task AgregarMesa()
        {
            var tipos = await _db.ObtenerTiposMesa();
            if (tipos.Count == 0)
            {
                await DisplayAlert("Tipos requeridos", "Primero crea al menos un tipo de mesa.", "OK");
                return;
            }
            int numeroMesa = await SolicitarEntero("Nueva mesa", "Número de mesa:", "1", true);
            if (numeroMesa <= 0) return;
            string[] nombres = tipos.Select(t => t.Nombre).ToArray();
            string seleccion = await DisplayActionSheet("Tipo de mesa", "Cancelar", null, nombres);
            var tipo = tipos.FirstOrDefault(t => t.Nombre == seleccion);
            if (tipo == null) return;
            await _db.GuardarMesa(new Mesa
            {
                NumeroMesa = numeroMesa,
                TipoMesaId = tipo.Id,
                TipoMesaNombre = tipo.Nombre,
                Ocupada = false,
                Cliente = "",
                Modalidad = ""
            });
            await CargarMesas();
        }

        private async Task AgregarTrabajador()
        {
            string nombre = await DisplayPromptAsync("Nuevo trabajador", "Nombre completo:", "Continuar", "Cancelar", "");
            if (string.IsNullOrWhiteSpace(nombre)) return;
            string usuario = await DisplayPromptAsync("Usuario", "Nombre de usuario:", "Continuar", "Cancelar", "");
            if (string.IsNullOrWhiteSpace(usuario)) return;
            string clave = await DisplayPromptAsync("Contraseña", "Contraseña:", "Guardar", "Cancelar", "123456");
            if (string.IsNullOrWhiteSpace(clave)) return;
            await _db.GuardarUsuario(new Usuario
            {
                Nombre = nombre.Trim(),
                UsuarioLogin = usuario.Trim(),
                Contrasena = clave,
                Rol = "TRABAJADOR"
            });
            await CargarTrabajadores();
        }

        private async Task AgregarCategoria()
        {
            string nombre = await DisplayPromptAsync("Nueva categoría", "Nombre:", "Guardar", "Cancelar", "");
            if (string.IsNullOrWhiteSpace(nombre)) return;
            await _db.GuardarCategoria(new CategoriaProducto { Nombre = nombre.Trim(), Activo = true });
            await CargarCategorias();
        }

        private async Task AgregarProducto()
        {
            var categorias = await _db.ObtenerCategorias();
            if (categorias.Count == 0)
            {
                await DisplayAlert("Categorías requeridas", "Primero crea al menos una categoría.", "OK");
                return;
            }

            string nombre = await DisplayPromptAsync("Nuevo producto", "Nombre del producto:", "Continuar", "Cancelar", "");
            if (string.IsNullOrWhiteSpace(nombre)) return;
            string categoriaNombre = await DisplayActionSheet("Categoría", "Cancelar", null, categorias.Select(c => c.Nombre).ToArray());
            var categoria = categorias.FirstOrDefault(c => c.Nombre == categoriaNombre);
            if (categoria == null) return;
            int cantidadCajas = await SolicitarEntero(
                "Cantidad de cajas",
                "¿Cuántas cajas ingresan al inventario?",
                "1",
                true);
            if (cantidadCajas <= 0) return;

            int unidadesPorCaja = await SolicitarEntero(
                "Unidades por caja",
                "¿Cuántas unidades individuales trae cada caja?",
                "12",
                true);
            if (unidadesPorCaja <= 0) return;

            decimal precioPorCaja = await SolicitarDecimal(
                "Costo de compra",
                "¿A cuánto se compró cada caja?",
                "0",
                false);
            if (precioPorCaja < 0) return;

            decimal precioVenta = await SolicitarDecimal(
                "Precio de venta",
                "¿A cuánto se venderá cada unidad?",
                "0",
                false);
            if (precioVenta < 0) return;

            int stockInicial = cantidadCajas * unidadesPorCaja;
            decimal costoTotalCompra = cantidadCajas * precioPorCaja;
            decimal costoUnitario = costoTotalCompra / stockInicial;

            string imagenRuta = "";
            try
            {
                var archivo = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Seleccionar imagen del producto",
                    FileTypes = FilePickerFileType.Images
                });
                if (archivo != null)
                {
                    string destino = Path.Combine(FileSystem.AppDataDirectory, $"{Guid.NewGuid():N}{Path.GetExtension(archivo.FileName)}");
                    using var fuente = await archivo.OpenReadAsync();
                    using var salida = File.Create(destino);
                    await fuente.CopyToAsync(salida);
                    imagenRuta = destino;
                }
            }
            catch
            {
                imagenRuta = "";
            }

            decimal gananciaUnitaria = precioVenta - costoUnitario;
            bool confirmar = await DisplayAlert(
                "Confirmar operación",
                $"{cantidadCajas} cajas × {unidadesPorCaja} unidades = {stockInicial} unidades\n" +
                $"Costo total: {costoTotalCompra:0.00} Bs\n" +
                $"Costo unitario: {costoUnitario:0.00} Bs\n" +
                $"Venta: {precioVenta:0.00} Bs\n" +
                $"Ganancia por unidad: {gananciaUnitaria:0.00} Bs",
                "Registrar", "Cancelar");
            if (!confirmar) return;

            var productoNuevo = new Producto
            {
                Nombre = nombre.Trim(),
                CategoriaId = categoria.Id,
                CategoriaNombre = categoria.Nombre,
                Stock = stockInicial,
                Precio = precioVenta,
                PrecioVentaUnidad = precioVenta,
                CajasCompra = cantidadCajas,
                UnidadesPorCaja = unidadesPorCaja,
                UnidadesPorPaquete = stockInicial,
                PrecioCompraCaja = precioPorCaja,
                PrecioCompraPaquete = costoTotalCompra,
                ImagenRuta = imagenRuta
            };
            await _db.GuardarProducto(productoNuevo);

            await _db.RegistrarCompra(new CompraProducto
            {
                ProductoId = productoNuevo.Id,
                NombreProducto = productoNuevo.Nombre,
                Cajas = cantidadCajas,
                UnidadesPorCaja = unidadesPorCaja,
                TotalUnidades = stockInicial,
                PrecioPorCaja = precioPorCaja,
                CostoTotal = costoTotalCompra,
                PrecioVentaUnidad = precioVenta,
                Fecha = DateTime.Now,
                Usuario = "Administrador"
            });

            await _db.RegistrarAuditoria(new RegistroAuditoria
            {
                Fecha = DateTime.Now,
                Usuario = "Administrador",
                Rol = "ADMIN",
                Accion = "ALTA DE PRODUCTO",
                Entidad = "Producto",
                Detalle = $"{nombre.Trim()}: {cantidadCajas} cajas, {unidadesPorCaja} unidades por caja, {stockInicial} unidades, costo {costoTotalCompra:0.00} Bs"
            });
            await CargarProductos();
        }

        private async void SeleccionarRegistro(object sender, SelectionChangedEventArgs e)
        {
            cvDatos.SelectedItem = null;
            if (e.CurrentSelection.FirstOrDefault() is not AdminRow fila || fila.Entidad == null) return;

            if (fila.Entidad is CierreCaja cierre && _modulo is "caja" or "auditoria")
            {
                string opcionCierre = await DisplayActionSheet(
                    $"Turno de {cierre.Trabajador}",
                    "Cancelar",
                    null,
                    "Ver detalle",
                    "Exportar PDF");

                if (opcionCierre == "Ver detalle")
                    await MostrarDetalleCierre(cierre);
                else if (opcionCierre == "Exportar PDF")
                    await ExportarPdf(new List<CierreCaja> { cierre });
                return;
            }

            string accion = await DisplayActionSheet(fila.Titulo, "Cancelar", null, "Editar", "Eliminar");
            if (accion == "Editar") await EditarRegistro(fila.Entidad);
            else if (accion == "Eliminar")
            {
                bool confirmar = await DisplayAlert("Confirmar", $"¿Eliminar {fila.Titulo}?", "Sí", "No");
                if (confirmar) await EliminarRegistro(fila.Entidad);
            }
        }

        private async Task EditarRegistro(object entidad)
        {
            if (entidad is TipoMesa tipo)
            {
                string nombre = await DisplayPromptAsync("Editar tipo", "Nombre:", "Guardar", "Cancelar", tipo.Nombre);
                if (string.IsNullOrWhiteSpace(nombre)) return;
                decimal precio = await SolicitarDecimal(
                    "Tarifa",
                    "Precio por hora:",
                    tipo.PrecioPorHora.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    true);
                if (precio < 0) return;
                tipo.Nombre = nombre.Trim();
                tipo.PrecioPorHora = precio;
                await _db.GuardarTipoMesa(tipo);
                await CargarTipos();
            }
            else if (entidad is Mesa mesa)
            {
                var tipos = await _db.ObtenerTiposMesa();
                string seleccion = await DisplayActionSheet("Tipo de mesa", "Cancelar", null, tipos.Select(t => t.Nombre).ToArray());
                var tipoSeleccionado = tipos.FirstOrDefault(t => t.Nombre == seleccion);
                if (tipoSeleccionado == null) return;
                mesa.TipoMesaId = tipoSeleccionado.Id;
                mesa.TipoMesaNombre = tipoSeleccionado.Nombre;
                await _db.GuardarMesa(mesa);
                await CargarMesas();
            }
            else if (entidad is Usuario usuario)
            {
                string nombre = await DisplayPromptAsync("Editar trabajador", "Nombre:", "Guardar", "Cancelar", usuario.Nombre);
                if (string.IsNullOrWhiteSpace(nombre)) return;
                usuario.Nombre = nombre.Trim();
                await _db.GuardarUsuario(usuario);
                await CargarTrabajadores();
            }
            else if (entidad is CategoriaProducto categoria)
            {
                string nombre = await DisplayPromptAsync("Editar categoría", "Nombre:", "Guardar", "Cancelar", categoria.Nombre);
                if (string.IsNullOrWhiteSpace(nombre)) return;
                categoria.Nombre = nombre.Trim();
                await _db.GuardarCategoria(categoria);
                await CargarCategorias();
            }
            else if (entidad is Producto producto)
            {
                decimal precioVenta = await SolicitarDecimal(
                    "Editar producto",
                    "Precio de venta:",
                    producto.PrecioVentaUnidad.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    false);
                if (precioVenta < 0) return;
                int stockActual = await SolicitarEntero(
                    "Inventario",
                    "Stock actual:",
                    producto.Stock.ToString(),
                    false);
                if (stockActual < 0) return;
                producto.PrecioVentaUnidad = precioVenta;
                producto.Precio = precioVenta;
                producto.Stock = stockActual;
                await _db.GuardarProducto(producto);
                await CargarProductos();
            }
            await _db.RegistrarAuditoria(new RegistroAuditoria
            {
                Fecha = DateTime.Now,
                Usuario = "Administrador",
                Rol = "ADMIN",
                Accion = "MODIFICACIÓN",
                Entidad = entidad.GetType().Name,
                Detalle = $"Registro actualizado: {entidad}"
            });
        }

        private async Task EliminarRegistro(object entidad)
        {
            string detalle = "";
            if (entidad is TipoMesa tipo)
            {
                detalle = $"Tipo de mesa: {tipo.Nombre}";
                await _db.EliminarTipoMesa(tipo);
            }
            else if (entidad is Mesa mesa)
            {
                detalle = $"Mesa: {mesa.NumeroMesa}";
                await _db.EliminarMesa(mesa);
            }
            else if (entidad is Usuario usuario)
            {
                detalle = $"Trabajador: {usuario.Nombre}";
                await _db.EliminarUsuario(usuario);
            }
            else if (entidad is CategoriaProducto categoria)
            {
                detalle = $"Categoría: {categoria.Nombre}";
                await _db.EliminarCategoria(categoria);
            }
            else if (entidad is Producto producto)
            {
                detalle = $"Producto: {producto.Nombre}";
                await _db.EliminarProducto(producto);
            }
            await _db.RegistrarAuditoria(new RegistroAuditoria
            {
                Fecha = DateTime.Now,
                Usuario = "Administrador",
                Rol = "ADMIN",
                Accion = "ELIMINACIÓN",
                Entidad = entidad.GetType().Name,
                Detalle = detalle
            });
            await CambiarModulo(_modulo);
        }
    }
}
