namespace TellTeddie.Web.Services
{
    /// <summary>
    /// Service to track whether an error has occurred and was the reason for navigation.
    /// This prevents direct access to error pages via URL.
    /// </summary>
    public interface IErrorStateService
    {
        /// <summary>
        /// Gets a value indicating whether an error has been triggered.
        /// </summary>
        bool HasError { get; }

        /// <summary>
        /// Sets the error state to indicate an error has occurred.
        /// </summary>
        void SetError();

        /// <summary>
        /// Clears the error state.
        /// </summary>
        void ClearError();
    }

    public class ErrorStateService : IErrorStateService
    {
        private bool _hasError = false;

        public bool HasError => _hasError;

        public void SetError()
        {
            _hasError = true;
        }

        public void ClearError()
        {
            _hasError = false;
        }
    }
}
