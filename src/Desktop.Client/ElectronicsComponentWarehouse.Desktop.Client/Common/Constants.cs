namespace ElectronicsComponentWarehouse.Desktop.Client.Common
{
    /// <summary>
    /// Константы приложения
    /// </summary>
    public static class Constants
    {
        public static class Api
        {
            public const string BaseUrl = "https://localhost:5001";
            public const string LoginEndpoint = "/api/Auth/login";
            public const string ValidateEndpoint = "/api/Auth/validate";
            public const string ComponentsEndpoint = "/api/Components";
            public const string CategoriesEndpoint = "/api/Categories";
            public const string UsersEndpoint = "/api/Users";
        }

        public static class Storage
        {
            public const string AuthTokenKey = "auth_token";
            public const string UserInfoKey = "user_info";
            public const string SettingsKey = "app_settings";
        }

        public static class Roles
        {
            public const string Admin = "Admin";
            public const string User = "User";
        }

        public static class Messages
        {
            public const string ConnectionError = "Ошибка подключения к серверу";
            public const string AuthenticationRequired = "Требуется аутентификация";
            public const string InsufficientPermissions = "Недостаточно прав";
            public const string OperationSuccess = "Операция выполнена успешно";
            public const string OperationFailed = "Ошибка при выполнении операции";
            public const string ConfirmDelete = "Вы уверены, что хотите удалить?";
            public const string Loading = "Загрузка...";
            public const string Saving = "Сохранение...";
        }

        public static class Validation
        {
            public const int MinUsernameLength = 3;
            public const int MaxUsernameLength = 50;
            public const int MinPasswordLength = 6;
            public const int MaxPasswordLength = 100;
            public const int MinComponentNameLength = 2;
            public const int MaxComponentNameLength = 200;
            public const int MinCategoryNameLength = 2;
            public const int MaxCategoryNameLength = 100;
        }
    }
}