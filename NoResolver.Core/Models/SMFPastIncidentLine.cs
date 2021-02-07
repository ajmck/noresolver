using Prism.Mvvm;

namespace NoResolver.Core.Models
{
    public class SMFPastIncidentLine : BindableBase
    {

        private string _timestamp;
        private string _inc;
        private string _error;
        private string _errorDescription;

        public string Timestamp
        {
            get { return _timestamp; }
            set { SetProperty(ref _timestamp, value);  }
        }

        public string INC
        {
            get { return _inc; }
            set { SetProperty(ref _inc, value); }
        }

        public string Error
        {
            get { return _error; }
            set { SetProperty(ref _error, value); }
        }


        public string ErrorDescription
        {
            get { return _errorDescription; }
            set { SetProperty(ref _errorDescription, value); }
        }


        public SMFPastIncidentLine(string timestamp, string inc, string error, string errorDescription)
        {
            Timestamp = timestamp;
            INC = inc;
            Error = error;
            ErrorDescription = errorDescription;
        }
    }
}
