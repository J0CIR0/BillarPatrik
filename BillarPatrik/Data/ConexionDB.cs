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

            InicializarUsuarios();
            InicializarMesas();
            InicializarProductos();
        }

        private void InicializarUsuarios()
        {
            var usuarios = _db.Table<Usuario>().ToListAsync().Result;
            if (usuarios.Count == 0)
            {
                _db.InsertAsync(new Usuario { Nombre = "Administrador", UsuarioLogin = "admin", Contrasena = "admin123", Rol = "ADMIN" }).Wait();
                _db.InsertAsync(new Usuario { Nombre = "Josue Claros", UsuarioLogin = "josue", Contrasena = "123456", Rol = "TRABAJADOR" }).Wait();
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
                    new Producto { Nombre = "Coca-Cola 500ml", Precio = 5.00m, Stock = 20 },
                    new Producto { Nombre = "Pepsi 500ml", Precio = 5.00m, Stock = 20 },
                    new Producto { Nombre = "Agua 500ml", Precio = 3.00m, Stock = 20 },
                    new Producto { Nombre = "Papas Fritas", Precio = 4.00m, Stock = 15 },
                    new Producto { Nombre = "Maní", Precio = 2.00m, Stock = 15 },
                    new Producto { Nombre = "Cerveza Paceña", Precio = 10.00m, Stock = 10 },
                    new Producto { Nombre = "Cerveza Huari", Precio = 12.00m, Stock = 10 },
                    new Producto { Nombre = "Coca Machucada", Precio = 8.00m, Stock = 10 },
                    new Producto { Nombre = "Singani 500ml", Precio = 25.00m, Stock = 5 },
                    new Producto { Nombre = "Salteña", Precio = 5.00m, Stock = 10 }
                };
                _db.InsertAllAsync(lista).Wait();
            }
        }

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