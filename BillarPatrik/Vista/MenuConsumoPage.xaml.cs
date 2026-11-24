using BillarPatrik.Modelo;

namespace BillarPatrik.Vista
{
    public partial class MenuConsumoPage : ContentPage
    {
        private readonly List<ProductoConsumo> _productos;
        private readonly Func<Producto, Task> _agregar;

        public MenuConsumoPage(List<Producto> productos, Func<Producto, Task> agregar)
        {
            InitializeComponent();
            _productos = productos.Select(CrearItem).ToList();
            _agregar = agregar;

            CrearCategorias();
            MostrarCategoria("Todos");
        }

        private void CrearCategorias()
        {
            string[] categorias = { "Todos", "Bebidas", "Snacks", "Cervezas", "Licores", "Comidas" };

            foreach (string categoria in categorias)
            {
                var boton = new Button
                {
                    Text = categoria,
                    BackgroundColor = categoria == "Todos" ? Color.FromArgb("#22C55E") : Color.FromArgb("#374151"),
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    CornerRadius = 18,
                    HeightRequest = 40,
                    Padding = new Thickness(14, 0)
                };

                boton.Clicked += (sender, e) =>
                {
                    foreach (var control in categoriasLayout.Children.OfType<Button>())
                        control.BackgroundColor = Color.FromArgb("#374151");

                    boton.BackgroundColor = Color.FromArgb("#22C55E");
                    MostrarCategoria(categoria);
                };

                categoriasLayout.Add(boton);
            }
        }

        private void MostrarCategoria(string categoria)
        {
            productosCarousel.ItemsSource = categoria == "Todos"
                ? _productos
                : _productos.Where(p => p.Categoria == categoria).ToList();
        }

        private async void CerrarMenu(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void AgregarProducto(object sender, EventArgs e)
        {
            if (sender is Button boton && boton.BindingContext is ProductoConsumo item)
            {
                await _agregar(item.Producto);
                await DisplayAlert("Consumo agregado", $"Se agregó {item.Nombre}.", "OK");
            }
        }

        private static ProductoConsumo CrearItem(Producto producto)
        {
            string nombre = producto.Nombre.ToUpperInvariant();
            string categoria;
            string imagen;

            if (nombre.Contains("CERVEZA"))
            {
                categoria = "Cervezas";
                imagen = "categoria_cervezas.png";
            }
            else if (nombre.Contains("SINGANI") || nombre.Contains("LICOR") || nombre.Contains("RON") || nombre.Contains("WHISKY") || nombre.Contains("VIN") )
            {
                categoria = "Licores";
                imagen = "categoria_licores.png";
            }
            else if (nombre.Contains("PAPAS") || nombre.Contains("MANI") || nombre.Contains("MANÍ") || nombre.Contains("SNACK"))
            {
                categoria = "Snacks";
                imagen = "categoria_snacks.png";
            }
            else if (nombre.Contains("SALTEÑA") || nombre.Contains("EMPANADA") || nombre.Contains("HAMBURG" ) || nombre.Contains("COMIDA") || nombre.Contains("SANDWICH") || nombre.Contains("SÁNDWICH"))
            {
                categoria = "Comidas";
                imagen = "categoria_comidas.png";
            }
            else
            {
                categoria = "Bebidas";
                imagen = "categoria_bebidas.png";
            }

            return new ProductoConsumo(producto, categoria, imagen);
        }

        private sealed class ProductoConsumo
        {
            public ProductoConsumo(Producto producto, string categoria, string imagen)
            {
                Producto = producto;
                Categoria = categoria;
                Imagen = imagen;
            }

            public Producto Producto { get; }
            public string Nombre => Producto.Nombre;
            public string PrecioTexto => $"{Producto.Precio:0.00} Bs";
            public string Categoria { get; }
            public string Imagen { get; }
        }
    }
}
