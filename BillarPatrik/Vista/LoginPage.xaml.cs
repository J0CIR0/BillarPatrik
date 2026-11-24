using BillarPatrik.Data;

namespace BillarPatrik.Vista
{
    public partial class LoginPage : ContentPage
    {
        private readonly ConexionDB _db = new ConexionDB();

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void Ingresar(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                lblError.Text = "Complete todos los campos";
                ErrorBorder.IsVisible = true;
                return;
            }

            var usuario = await _db.ValidarLogin(txtUsuario.Text, txtContrasena.Text);

            if (usuario == null)
            {
                await _db.RegistrarAuditoria(new Modelo.RegistroAuditoria
                {
                    Fecha = DateTime.Now,
                    Usuario = txtUsuario.Text.Trim(),
                    Rol = "",
                    Accion = "INICIO DE SESIÓN FALLIDO",
                    Entidad = "Sesión",
                    Detalle = "Credenciales incorrectas"
                });
                lblError.Text = "Credenciales incorrectas";
                ErrorBorder.IsVisible = true;
                return;
            }

            ErrorBorder.IsVisible = false;
            await _db.RegistrarAuditoria(new Modelo.RegistroAuditoria
            {
                Fecha = DateTime.Now,
                Usuario = usuario.Nombre,
                Rol = usuario.Rol,
                Accion = "INICIO DE SESIÓN",
                Entidad = "Sesión",
                Detalle = $"Acceso al panel {(usuario.Rol == "ADMIN" ? "administrador" : "trabajador")}"
            });

            if (usuario.Rol == "ADMIN")
            {
                await Navigation.PushAsync(new AdminPage(usuario.Nombre));
            }
            else
            {
                await Navigation.PushAsync(new MainPage(usuario.Nombre));
            }
        }
    }
}