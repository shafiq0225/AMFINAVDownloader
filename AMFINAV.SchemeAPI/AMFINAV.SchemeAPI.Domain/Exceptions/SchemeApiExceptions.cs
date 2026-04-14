namespace AMFINAV.SchemeAPI.Domain.Exceptions
{
    /// <summary>Base exception for all SchemeAPI errors.</summary>
    public class SchemeApiException : Exception
    {
        public string ErrorCode { get; }
        public int StatusCode { get; }

        public SchemeApiException(
            string message,
            string errorCode = "SCHEME_API_ERROR",
            int statusCode = 500)
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        public SchemeApiException(
            string message,
            Exception innerException,
            string errorCode = "SCHEME_API_ERROR",
            int statusCode = 500)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }

    /// <summary>Thrown when a requested resource is not found.</summary>
    public class NotFoundException : SchemeApiException
    {
        public NotFoundException(string resource, string identifier)
            : base($"{resource} '{identifier}' was not found.",
                  "NOT_FOUND", 404)
        { }
    }

    /// <summary>Thrown when a duplicate resource is detected.</summary>
    public class DuplicateException : SchemeApiException
    {
        public DuplicateException(string resource, string identifier)
            : base($"{resource} '{identifier}' already exists.",
                  "DUPLICATE_RESOURCE", 409)
        { }
    }

    /// <summary>Thrown when input validation fails.</summary>
    public class ValidationException : SchemeApiException
    {
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public ValidationException(Dictionary<string, string[]> errors)
            : base("One or more validation errors occurred.",
                  "VALIDATION_ERROR", 400)
        {
            Errors = errors;
        }
    }

    /// <summary>Thrown when NAV comparison data is unavailable.</summary>
    public class NavDataNotFoundException : SchemeApiException
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        public NavDataNotFoundException(DateTime startDate, DateTime endDate)
            : base($"No NAV data found between " +
                  $"{startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}.",
                  "NAV_DATA_NOT_FOUND", 404)
        {
            StartDate = startDate;
            EndDate = endDate;
        }
    }

    /// <summary>Thrown when SchemeEnrollment operation fails.</summary>
    public class SchemeEnrollmentException : SchemeApiException
    {
        public string SchemeCode { get; }

        public SchemeEnrollmentException(string message, string schemeCode)
            : base(message, "SCHEME_ENROLLMENT_ERROR", 400)
        {
            SchemeCode = schemeCode;
        }
    }

    /// <summary>Thrown when fund approval cascade fails.</summary>
    public class FundApprovalException : SchemeApiException
    {
        public string FundCode { get; }

        public FundApprovalException(string message, string fundCode)
            : base(message, "FUND_APPROVAL_ERROR", 400)
        {
            FundCode = fundCode;
        }
    }

    /// <summary>Thrown when NavFileConsumer processing fails.</summary>
    public class NavConsumerException : SchemeApiException
    {
        public DateTime NavDate { get; }

        public NavConsumerException(string message, DateTime navDate,
            Exception innerException)
            : base(message, innerException, "NAV_CONSUMER_ERROR", 500)
        {
            NavDate = navDate;
        }
    }
}