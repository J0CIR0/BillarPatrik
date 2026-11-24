using SQLite;
using BillarPatrik.Modelo;

namespace BillarPatrik.Data
{
    public class ConexionDB
    {
        private readonly SQLiteAsyncConnection _db;

        public ConexionDB()
        {
            string ruta = Path.Combine(FileSystem.AppDataDirectory, "BillarPatrik.db");
            _db = new SQLiteAsyncConnection(ruta, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            _db.CreateTableAsync<Usuario>().Wait();
            _db.CreateTableAsync<Mesa>().Wait();
            _db.CreateTableAsync<Producto>().Wait();
            _db.CreateTableAsync<Venta>().Wait();
            _db.CreateTableAsync<TipoMesa>().Wait();
            _db.CreateTableAsync<CategoriaProducto>().Wait();
            _db.CreateTableAsync<ConsumoMesa>().Wait();
            _db.CreateTableAsync<VentaProducto>().Wait();
            _db.CreateTableAsync<CierreCaja>().Wait();
            _db.CreateTableAsync<CompraProducto>().Wait();
            _db.CreateTableAsync<RegistroAuditoria>().Wait();
            try
            {
                _db.ExecuteAsync("ALTER TABLE CierresCaja ADD COLUMN FechaInicio TEXT").Wait();
            }
            catch { }
            try
            {
                _db.ExecuteAsync("ALTER TABLE CierresCaja ADD COLUMN IdsVentas TEXT").Wait();
            }
            catch { }

            InicializarUsuarios();
            InicializarMesas();
            InicializarTiposYCategorias();
            InicializarProductos();
        }

        private void InicializarUsuarios()
        {
            var usuarios = _db.Table<Usuario>().ToListAsync().Result;
            if (!usuarios.Any(u => u.Rol == "ADMIN"))
            {
                _db.InsertAsync(new Usuario { Nombre = "Administrador", UsuarioLogin = "admin", Contrasena = "admin123", Rol = "ADMIN" }).Wait();
            }
        }

        private void InicializarMesas()
        {
            var mesas = _db.Table<Mesa>().ToListAsync().Result;
            if (mesas.Count == 0)
            {
                for (int i = 1; i <= 10; i++)
                {
                    var mesa = new Mesa
                    {
                        NumeroMesa = i,
                        Ocupada = false,
                        Cliente = "",
                        Modalidad = "",
                        ConsumoTotal = 0
                    };
                    _db.InsertAsync(mesa).Wait();
                }
            }
        }

        private void InicializarProductos()
        {
            var productos = _db.Table<Producto>().ToListAsync().Result;
            if (productos.Count == 0)
            {
                var lista = new List<Producto>
                {
                    new Producto { Nombre = "Coca-Cola 500ml", Precio = 5.00m, PrecioVentaUnidad = 5.00m, Stock = 20, UnidadesPorPaquete = 12, PrecioCompraPaquete = 48.00m },
                    new Producto { Nombre = "Pepsi 500ml", Precio = 5.00m, PrecioVentaUnidad = 5.00m, Stock = 20, UnidadesPorPaquete = 12, PrecioCompraPaquete = 48.00m },
                    new Producto { Nombre = "Agua 500ml", Precio = 3.00m, PrecioVentaUnidad = 3.00m, Stock = 20, UnidadesPorPaquete = 12, PrecioCompraPaquete = 30.00m },
                    new Producto { Nombre = "Papas Fritas", Precio = 4.00m, PrecioVentaUnidad = 4.00m, Stock = 15, UnidadesPorPaquete = 10, PrecioCompraPaquete = 25.00m },
                    new Producto { Nombre = "Maní", Precio = 2.00m, PrecioVentaUnidad = 2.00m, Stock = 15, UnidadesPorPaquete = 20, PrecioCompraPaquete = 24.00m },
                    new Producto { Nombre = "Cerveza Paceña", Precio = 10.00m, PrecioVentaUnidad = 10.00m, Stock = 10, UnidadesPorPaquete = 12, PrecioCompraPaquete = 96.00m },
                    new Producto { Nombre = "Cerveza Huari", Precio = 12.00m, PrecioVentaUnidad = 12.00m, Stock = 10, UnidadesPorPaquete = 12, PrecioCompraPaquete = 108.00m },
                    new Producto { Nombre = "Coca Machucada", Precio = 8.00m, PrecioVentaUnidad = 8.00m, Stock = 10, UnidadesPorPaquete = 10, PrecioCompraPaquete = 60.00m },
                    new Producto { Nombre = "Singani 500ml", Precio = 25.00m, PrecioVentaUnidad = 25.00m, Stock = 5, UnidadesPorPaquete = 6, PrecioCompraPaquete = 120.00m },
                    new Producto { Nombre = "Salteña", Precio = 5.00m, PrecioVentaUnidad = 5.00m, Stock = 10, UnidadesPorPaquete = 10, PrecioCompraPaquete = 35.00m }
                };
                _db.InsertAllAsync(lista).Wait();
            }
        }

        private void InicializarTiposYCategorias()
        {
            if (_db.Table<TipoMesa>().CountAsync().Result == 0)
            {
                _db.InsertAllAsync(new[]
                {
                    new TipoMesa { Nombre = "Mesa normal", PrecioPorHora = 30m, Activo = true },
                    new TipoMesa { Nombre = "Sala privada", PrecioPorHora = 60m, Activo = true }
                }).Wait();

                var tipos = _db.Table<TipoMesa>().ToListAsync().Result;
                var mesas = _db.Table<Mesa>().ToListAsync().Result;
                foreach (var mesa in mesas)
                {
                    mesa.TipoMesaId = tipos.First().Id;
                    mesa.TipoMesaNombre = tipos.First().Nombre;
                    _db.UpdateAsync(mesa).Wait();
                }
            }

            if (_db.Table<CategoriaProducto>().CountAsync().Result == 0)
            {
                var categorias = new[]
                {
                    new CategoriaProducto { Nombre = "Bebidas", Activo = true },
                    new CategoriaProducto { Nombre = "Snacks", Activo = true },
                    new CategoriaProducto { Nombre = "Cerveza", Activo = true },
                    new CategoriaProducto { Nombre = "Comida", Activo = true }
                };
                _db.InsertAllAsync(categorias).Wait();
            }
        }

        public async Task<List<TipoMesa>> ObtenerTiposMesa() =>
            await _db.Table<TipoMesa>().ToListAsync();

        public async Task<int> GuardarTipoMesa(TipoMesa tipo) =>
            tipo.Id == 0 ? await _db.InsertAsync(tipo) : await _db.UpdateAsync(tipo);

        public async Task<int> EliminarTipoMesa(TipoMesa tipo) =>
            await _db.DeleteAsync(tipo);

        public async Task<int> GuardarMesa(Mesa mesa) =>
            mesa.Id == 0 ? await _db.InsertAsync(mesa) : await _db.UpdateAsync(mesa);

        public async Task<int> EliminarMesa(Mesa mesa) =>
            await _db.DeleteAsync(mesa);

        public async Task<List<Usuario>> ObtenerTrabajadores() =>
            await _db.Table<Usuario>().Where(u => u.Rol == "TRABAJADOR").ToListAsync();

        public async Task<List<Usuario>> ObtenerUsuarios() =>
            await _db.Table<Usuario>().ToListAsync();

        public async Task<int> GuardarUsuario(Usuario usuario) =>
            usuario.Id == 0 ? await _db.InsertAsync(usuario) : await _db.UpdateAsync(usuario);

        public async Task<int> EliminarUsuario(Usuario usuario) =>
            await _db.DeleteAsync(usuario);

        public async Task<List<CategoriaProducto>> ObtenerCategorias() =>
            await _db.Table<CategoriaProducto>().ToListAsync();

        public async Task<int> GuardarCategoria(CategoriaProducto categoria) =>
            categoria.Id == 0 ? await _db.InsertAsync(categoria) : await _db.UpdateAsync(categoria);

        public async Task<int> EliminarCategoria(CategoriaProducto categoria) =>
            await _db.DeleteAsync(categoria);

        public async Task<int> GuardarProducto(Producto producto) =>
            producto.Id == 0 ? await _db.InsertAsync(producto) : await _db.UpdateAsync(producto);

        public async Task<int> EliminarProducto(Producto producto) =>
            await _db.DeleteAsync(producto);

        public async Task<int> AgregarConsumoMesa(ConsumoMesa consumo) =>
            await _db.InsertAsync(consumo);

        public async Task<List<ConsumoMesa>> ObtenerConsumosMesa(int mesaId) =>
            await _db.Table<ConsumoMesa>().Where(c => c.MesaId == mesaId).ToListAsync();

        public async Task LimpiarConsumosMesa(int mesaId)
        {
            var consumos = await ObtenerConsumosMesa(mesaId);
            foreach (var consumo in consumos)
                await _db.DeleteAsync(consumo);
        }

        public async Task<List<VentaProducto>> ObtenerVentasProducto() =>
            await _db.Table<VentaProducto>().ToListAsync();

        public async Task<int> InsertarVentaProducto(VentaProducto ventaProducto) =>
            await _db.InsertAsync(ventaProducto);

        public async Task<List<Venta>> ObtenerTodasLasVentas() =>
            await _db.Table<Venta>().ToListAsync();

        public async Task<List<CierreCaja>> ObtenerCierresCaja() =>
            await _db.Table<CierreCaja>().OrderByDescending(c => c.Fecha).ToListAsync();

        public async Task RegistrarCierre(CierreCaja cierre) =>
            await _db.InsertAsync(cierre);

        public async Task<List<Venta>> ObtenerVentasDeCierre(CierreCaja cierre)
        {
            var ids = cierre.IdsVentas?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(v => int.TryParse(v, out int id) ? id : 0)
                .Where(id => id > 0)
                .ToHashSet() ?? new HashSet<int>();

            var todas = await _db.Table<Venta>().ToListAsync();
            if (ids.Count > 0)
                return todas.Where(v => ids.Contains(v.Id)).ToList();

            return todas.Where(v => v.Trabajador == cierre.Trabajador &&
                                    v.Fecha >= (cierre.FechaInicio == default ? cierre.Fecha.AddHours(-12) : cierre.FechaInicio) &&
                                    v.Fecha <= cierre.Fecha).ToList();
        }

        public async Task<DateTime> ObtenerInicioUltimoTurno(string trabajador)
        {
            var registros = await _db.Table<RegistroAuditoria>()
                .Where(r => r.Usuario == trabajador &&
                            r.Rol == "TRABAJADOR" &&
                            r.Accion == "INICIO DE SESIÓN")
                .ToListAsync();
            var registro = registros.OrderByDescending(r => r.Fecha).FirstOrDefault();

            return registro?.Fecha ?? DateTime.Now;
        }

        public async Task<int> RegistrarCompra(CompraProducto compra) =>
            await _db.InsertAsync(compra);

        public async Task<List<CompraProducto>> ObtenerComprasProducto() =>
            await _db.Table<CompraProducto>().OrderByDescending(c => c.Fecha).ToListAsync();

        public async Task<int> RegistrarAuditoria(RegistroAuditoria registro) =>
            await _db.InsertAsync(registro);

        public async Task<List<RegistroAuditoria>> ObtenerAuditoria() =>
            await _db.Table<RegistroAuditoria>().OrderByDescending(a => a.Fecha).ToListAsync();

        public async Task<Usuario> ValidarLogin(string usuario, string contrasena)
        {
            return await _db.Table<Usuario>().FirstOrDefaultAsync(u => u.UsuarioLogin == usuario && u.Contrasena == contrasena);
        }

        public async Task<List<Mesa>> ObtenerMesas()
        {
            return await _db.Table<Mesa>().ToListAsync();
        }

        public async Task<int> ActualizarMesa(Mesa mesa)
        {
            return await _db.UpdateAsync(mesa);
        }

        public async Task<List<Producto>> ObtenerProductos()
        {
            return await _db.Table<Producto>().ToListAsync();
        }

        public async Task<int> GuardarVenta(Venta venta)
        {
            return await _db.InsertAsync(venta);
        }

        public async Task<List<Venta>> ObtenerVentasPorTrabajador(string trabajador)
        {
            return await _db.Table<Venta>().Where(v => v.Trabajador == trabajador && !v.Entregado).ToListAsync();
        }

        public async Task<List<Venta>> ObtenerTodasLasVentasPendientes()
        {
            return await _db.Table<Venta>().Where(v => !v.Entregado).ToListAsync();
        }

        public async Task CerrarCaja(string trabajador)
        {
            var ventas = await _db.Table<Venta>().Where(v => v.Trabajador == trabajador && !v.Entregado).ToListAsync();
            foreach (var venta in ventas)
            {
                venta.Entregado = true;
                await _db.UpdateAsync(venta);
            }
        }
    }
}