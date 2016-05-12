﻿using Xunit;
using WebLedMatrix.Hubs;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Autofac.Integration.SignalR;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Moq;
using NSubstitute;
using NSubstitute.Extensions;
using NSubstitute.ReturnsExtensions;
using WebLedMatrix;
using WebLedMatrix.Logic.Authentication.Abstract;
using WebLedMatrix.Logic.Authentication.Models;
using WebLedMatrixTests1;
using CallInfo = NSubstitute.Core.CallInfo;

namespace Hubs.Tests
{
    public class UiManagerTests : BaseTest
    {
        private readonly ILoginStatusChecker _loginStatusChecker = new LoginStatusChecker();


        static IRequest GetIdentityRequest(bool isAuthenticated)
        {
            var request = Substitute.For<IRequest>();
            
            request.User.Identity.IsAuthenticated.Returns(isAuthenticated);

            return request;
        }

        public void CoreAccountTest(State expectedState, IRequest identityRequest)
        {

            var matrixManager = Substitute.For<MatrixManager>();
            matrixManager.When(x=>x.UpdateMatrices()).DoNotCallBase();

            UiManagerHub managerHub = Substitute.For<UiManagerHub>(_loginStatusChecker, matrixManager);
            managerHub.Context = new HubCallerContext(identityRequest,"1");
            managerHub.Clients = Substitute.For<IHubCallerConnectionContext<IUiManagerHub>>();

            string result = "";
            managerHub.Clients.When(x => { var r = x.Caller; }).DoNotCallBase();
            managerHub.Clients.Caller.WhenForAnyArgs(x=>x.loginStatus("")).Do(x=> { result = x[0].ToString(); });

            managerHub.LoginStatus();
            Assert.Equal(expectedState.ToString(),result);
        }

        [Fact()]
        public void NotLoggedCaseTest()
        {
            CoreAccountTest(State.NotLogged, GetIdentityRequest(false));
        }

        [Fact()]
        public void LoggedCaseTest()
        {
            CoreAccountTest(State.Logged, GetIdentityRequest(true));
        }

    }
}