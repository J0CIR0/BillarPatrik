namespace BillarPatrik.Data
{
    public static class ApiSettings
    {
        public const string BaseAddress = "http://192.168.0.6:5025";
        public const string HubPath = "/billarhub";

        public static Uri ApiUri => new(BaseAddress, UriKind.Absolute);
        public static Uri HubUri => new(new Uri(BaseAddress + "/", UriKind.Absolute), HubPath.TrimStart('/'));
    }
}
