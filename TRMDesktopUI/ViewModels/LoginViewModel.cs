using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRMDesktopUI.EventModels;
using TRMDesktopUI.Helpers;
using TRMDesktopUI.Library.Api;

namespace TRMDesktopUI.ViewModels
{
    public class LoginViewModel : Screen
    {
        private readonly IAPIHelper _aPIHelper;
        private readonly IEventAggregator _events;
        private string _userName = "trantuvan.kan@gmail.com";
        private string _password = "Pwd12345.";
        private bool _isErrorVisible;
        private string _errorMessage;

        public LoginViewModel(IAPIHelper aPIHelper, IEventAggregator events)
        {
            _aPIHelper = aPIHelper;
            _events = events;
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                NotifyOfPropertyChange(() => UserName);
                NotifyOfPropertyChange(() => CanLogIn);
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                NotifyOfPropertyChange(() => Password);
                NotifyOfPropertyChange(() => CanLogIn);
            }
        }

        public bool IsErrorVisible
        {
            get
            {
                bool output = false;

                if (ErrorMessage?.Length > 0)
                {
                    output = true;
                }

                return output;
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                NotifyOfPropertyChange(() => IsErrorVisible);
                NotifyOfPropertyChange(() => ErrorMessage);
            }
        }


        public bool CanLogIn
        {
            get
            {
                bool output = false;

                if (UserName?.Length > 0 && Password?.Length > 0)
                {
                    output = true;
                }
                return output;
            }
        }

        public async Task LogIn()
        {
            try
            {
                ErrorMessage = "";
                var result = await _aPIHelper.Authenticate(UserName, Password);

                //Capture more information about user
                await _aPIHelper.GetLoggedInUserInfo(result.Access_Token);

                //Publish event to know someone is loged in
                //pass in an instance of LogOnEvent to make sure this is a LogOnEvent
                //this LogOnEvent doesn't need to pass in any data because GetLoggedInUserInfo already done it
                await _events.PublishOnUIThreadAsync(new LogOnEvent());

            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
