using BillarPatrik.Data;

namespace BillarPatrik.Vista
{
    public partial class AdminPage : ContentPage
    {
        private readonly ConexionDB _db = new ConexionDB();

        public AdminPage(string nombre)
        {
            InitializeComponent();
            lblBienvenida.Text = $"Bienvenido, {nombre}";
        }

        private async void CerrarSesion(object sender, EventArgs e)
        {
            await _db.RegistrarAuditoria(new Modelo.RegistroAuditoria
            {
                Fecha = DateTime.Now,
                Usuario = lblBienvenida.Text.Replace("Bienvenido, ", ""),
                Rol = "ADMIN",
                Accion = "CIERRE DE SESIÓN",
                Entidad = "Sesión",
                Detalle = "El administrador cerró su sesión"
            });
            await Navigation.PopToRootAsync();
        }

        private async Task AbrirModulo(string modulo) =>
            await Navigation.PushAsync(new AdminManagementPage(modulo));

        private async void AbrirMesas(object sender, EventArgs e) =>
            await AbrirModulo("mesas");

        private async void AbrirTrabajadores(object sender, EventArgs e) =>
            await AbrirModulo("trabajadores");

        private async void AbrirProductos(object sender, EventArgs e) =>
            await AbrirModulo("productos");

        private async void AbrirCaja(object sender, EventArgs e) =>
            await AbrirModulo("caja");

        private async void AbrirContabilidad(object sender, EventArgs e) =>
            await AbrirModulo("contabilidad");

        private async void AbrirAuditoria(object sender, EventArgs e) =>
            await AbrirModulo("auditoria");
    }
}