namespace ApiResponseCode
{
    public static class ResponseCodes
    {
        // Authentication & Registration
        public const string REGISTRATION_SUCCESSFUL = "REGISTRATION_SUCCESSFUL";
        public const string LOGIN_SUCCESSFUL = "LOGIN_SUCCESSFUL";
        public const string EMAIL_CONFIRMED = "EMAIL_CONFIRMED";
        public const string PASSWORD_RESET_SENT = "PASSWORD_RESET_SENT";
        public const string PASSWORD_RESET_SUCCESSFUL = "PASSWORD_RESET_SUCCESSFUL";

        // User Management
        public const string USER_BANNED = "USER_BANNED";
        public const string USER_UNBANNED = "USER_UNBANNED";
        public const string AVAILABILITY_UPDATED = "AVAILABILITY_UPDATED";

        // Error Codes - Validation
        public const string VALIDATION_ERROR = "VALIDATION_ERROR";
        public const string REQUIRED_FIELD_MISSING = "REQUIRED_FIELD_MISSING";
        public const string INVALID_EMAIL_FORMAT = "INVALID_EMAIL_FORMAT";
        public const string INVALID_PHONE_FORMAT = "INVALID_PHONE_FORMAT";
        public const string INVALID_NATIONAL_NUMBER = "INVALID_NATIONAL_NUMBER";

        // Error Codes - Business Logic
        public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";
        public const string PHONE_ALREADY_EXISTS = "PHONE_ALREADY_EXISTS";
        public const string NATIONAL_NUMBER_EXISTS = "NATIONAL_NUMBER_EXISTS";
        public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";

        public const string INVALID_TOKEN = "INVALID_TOKEN";
        public const string EMAIL_NOT_CONFIRMED = "EMAIL_NOT_CONFIRMED";
        public const string ACCESS_DENIED = "ACCESS_DENIED";
        public const string INSUFFICIENT_PERMISSIONS = "INSUFFICIENT_PERMISSIONS";
        public const string UNAUTHORIZED = "UNAUTHORIZED";
        // Worker Specific
        public const string WORKER_SPECIALTY_REQUIRED = "WORKER_SPECIALTY_REQUIRED";
        public const string INVALID_SPECIALTY = "INVALID_SPECIALTY";
        public const string WORKERS_RETRIEVED = "WORKERS_RETRIEVED";
        public const string NO_WORKERS_AVAILABLE = "NO_WORKERS_AVAILABLE";

        // System Errors
        public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
        public const string DATABASE_ERROR = "DATABASE_ERROR";
        public const string EXTERNAL_SERVICE_ERROR = "EXTERNAL_SERVICE_ERROR";
        public const string EMAIL_SERVICE_ERROR = "EMAIL_SERVICE_ERROR";

        //Request States
        public const string REQUEST_CREATED = "REQUEST_CREATED";
        public const string REQUEST_ACCEPTED = "REQUEST_ACCEPTED";
        public const string REQUEST_REJECTED = "REQUEST_REJECTED";
        public const string REQUEST_COMPLETED = "REQUEST_COMPLETED";
        public const string REQUEST_CANCELLED = "REQUEST_CANCELLED";
        public const string REQUEST_NOT_FOUND = "REQUEST_NOT_FOUND";
        public const string REQUEST_FETCHED = "REQUEST_FETCHED";
        public const string REQUEST_STILL_IN_PROGRESS = "REQUEST_STILL_IN_PROGRESS";
        public const string INVALID_REQUEST_STATUS = "INVALID_REQUEST_STATUS";

        //Rating & Reporting
        public const string RATING_SUBMITTED = "RATING_SUBMITTED";
        public const string RATING_ALREADY_EXISTS = "RATING_ALREADY_EXISTS";
        public const string REPORT_SUBMITTED = "REPORT_SUBMITTED";
    }
}