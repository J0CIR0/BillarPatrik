namespace BillarPatrik.Vista
{
    public partial class AdminPage : ContentPage
    {
        public AdminPage(string nombre)
        {
            InitializeComponent();
            lblBienvenida.Text = $"Bienvenido, {nombre}";
        }

        private async void CerrarSesion(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}