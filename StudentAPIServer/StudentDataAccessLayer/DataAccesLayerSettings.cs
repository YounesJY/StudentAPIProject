namespace StudentDataAccessLayer
{
    public static class DataAccesLayerSettings
    {
        private static string? _connectionString;

        public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static string GetConnectionString()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException(
                    "Connection string has not been set. Call SetConnectionString at startup.");

            return _connectionString;
        }
    }
}
