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
                lblError.IsVisible = true;
                return;
            }

            var usuario = await _db.ValidarLogin(txtUsuario.Text, txtContrasena.Text);

            if (usuario == null)
            {
                lblError.Text = "Credenciales incorrectas";
                lblError.IsVisible = true;
                return;
            }

            lblError.IsVisible = false;

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