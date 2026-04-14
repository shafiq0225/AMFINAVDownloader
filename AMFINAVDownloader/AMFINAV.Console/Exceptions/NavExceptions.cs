namespace AMFINAV.Console.Exceptions
{
    /// <summary>Base exception for all AMFINAV errors.</summary>
    public class AmfiNavException : Exception
    {
        public string ErrorCode { get; }

        public AmfiNavException(string message, string errorCode = "AMFINAV_ERROR")
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public AmfiNavException(string message, Exception innerException,
            string errorCode = "AMFINAV_ERROR")
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>Thrown when NAV file download fails.</summary>
    public class NavDownloadException : AmfiNavException
    {
        public int AttemptNumber { get; }

        public NavDownloadException(string message, int attemptNumber)
            : base(message, "NAV_DOWNLOAD_FAILED")
        {
            AttemptNumber = attemptNumber;
        }

        public NavDownloadException(string message, int attemptNumber,
            Exception innerException)
            : base(message, innerException, "NAV_DOWNLOAD_FAILED")
        {
            AttemptNumber = attemptNumber;
        }
    }

    /// <summary>Thrown when database operation fails.</summary>
    public class NavStorageException : AmfiNavException
    {
        public DateTime NavDate { get; }

        public NavStorageException(string message, DateTime navDate)
            : base(message, "NAV_STORAGE_FAILED")
        {
            NavDate = navDate;
        }

        public NavStorageException(string message, DateTime navDate,
            Exception innerException)
            : base(message, innerException, "NAV_STORAGE_FAILED")
        {
            NavDate = navDate;
        }
    }

    /// <summary>Thrown when RabbitMQ publish fails.</summary>
    public class NavPublishException : AmfiNavException
    {
        public DateTime NavDate { get; }

        public NavPublishException(string message, DateTime navDate)
            : base(message, "NAV_PUBLISH_FAILED")
        {
            NavDate = navDate;
        }

        public NavPublishException(string message, DateTime navDate,
            Exception innerException)
            : base(message, innerException, "NAV_PUBLISH_FAILED")
        {
            NavDate = navDate;
        }
    }

    /// <summary>Thrown when holiday API fetch fails.</summary>
    public class HolidayFetchException : AmfiNavException
    {
        public HolidayFetchException(string message)
            : base(message, "HOLIDAY_FETCH_FAILED") { }

        public HolidayFetchException(string message, Exception innerException)
            : base(message, innerException, "HOLIDAY_FETCH_FAILED") { }
    }
}