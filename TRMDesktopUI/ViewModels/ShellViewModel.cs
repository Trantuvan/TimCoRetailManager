using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TRMDesktopUI.EventModels;
using TRMDesktopUI.Library.Api;
using TRMDesktopUI.Library.Models;

namespace TRMDesktopUI.ViewModels
{
    public class ShellViewModel : Conductor<object>, IHandle<LogOnEvent>
    {
        private readonly IEventAggregator _events;
        private readonly IAPIHelper _apiHelper;
        private readonly ILoggedInUserModel _user;

        public ShellViewModel(IEventAggregator events,
                              IAPIHelper apiHelper,
                              ILoggedInUserModel user)
        {
            _events = events;
            _apiHelper = apiHelper;
            _user = user;

            // who subcribe to event; ShellViewModel current instance per request
            _events.SubscribeOnPublishedThread(this);
            // IoC inversion of control talk to container to get Instances
            ActivateItemAsync(IoC.Get<LoginViewModel>(), new CancellationToken());
        }

        public bool IsLoggedIn
        {
            get
            {
                bool output = false;

                if (string.IsNullOrWhiteSpace(_user.Token) == false)
                {
                    output = true;
                }
                return output;
            }
        }
        public async Task ExitApplication()
        {
            await TryCloseAsync();
        }

        public async Task UserManagement()
        {
            await ActivateItemAsync(IoC.Get<UserDisplayViewModel>(), new CancellationToken());
        }

        public async Task LogOut()
        {
            _user.ResetUserModel();
            _apiHelper.LogOffUser();
            await ActivateItemAsync(IoC.Get<LoginViewModel>(), new CancellationToken());
            NotifyOfPropertyChange(() => IsLoggedIn);
        }

        public async Task HandleAsync(LogOnEvent message, CancellationToken cancellationToken)
        {
            // only 1 item at the time Conductor<object>
            await ActivateItemAsync(IoC.Get<SalesViewModel>(), cancellationToken);
            NotifyOfPropertyChange(() => IsLoggedIn);
        }
    }
}
