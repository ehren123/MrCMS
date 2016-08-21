using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using FakeItEasy;
using FluentAssertions;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Tests.TestSupport;
using MrCMS.Website;
using Xunit;

namespace MrCMS.Tests.Services
{
    public class UserLookupTests 
    {
        private readonly UserLookup _userService;
        private readonly InMemoryRepository<User> _inMemoryRepository;
        private readonly IGetNow _getNow = new TestGetNow(Now);
        private static readonly DateTime Now = new DateTime(2016,9,1);

        public UserLookupTests()
        {
            _inMemoryRepository = new InMemoryRepository<User>();
            _userService = new UserLookup(_inMemoryRepository, new List<IExternalUserSource>(),_getNow);
        }
        [Fact]
        public void UserService_GetUserByEmail_ReturnsNullWhenNoUserAvailable()
        {
            _userService.GetUserByEmail("test@example.com").Should().BeNull();
        }

        [Fact]
        public void UserService_GetUserByEmail_WithValidEmailReturnsTheCorrectUser()
        {
            var user = new User { FirstName = "Test", LastName = "User", Email = "test@example.com" };
            _inMemoryRepository.Add(user);
            var user2 = new User { FirstName = "Test", LastName = "User2", Email = "test2@example.com" };
            _inMemoryRepository.Add(user2);

            _userService.GetUserByEmail("test2@example.com").Should().Be(user2);
        }

        [Fact]
        public void UserService_GetUserByResetGuid_ReturnsNullForInvalidGuid()
        {
            _userService.GetUserByResetGuid(Guid.Empty).Should().BeNull();
        }

        [Fact]
        public void UserService_GetUserByResetGuid_ValidGuidButExpiryPassedReturnsNull()
        {
            var resetPasswordGuid = Guid.NewGuid();
            var user = new User
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                ResetPasswordGuid = resetPasswordGuid,
                ResetPasswordExpiry = DateTime.Now.AddDays(-2)
            };
            _inMemoryRepository.Add(user);

            _userService.GetUserByResetGuid(resetPasswordGuid).Should().BeNull();
        }

        [Fact]
        public void UserService_GetUserByResetGuid_ValidGuidAndExpiryInTheFutureReturnsUser()
        {
            var resetPasswordGuid = Guid.NewGuid();
            var user = new User
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                ResetPasswordGuid = resetPasswordGuid,
                ResetPasswordExpiry = Now.AddDays(1)
            };
            _inMemoryRepository.Add(user);

            _userService.GetUserByResetGuid(resetPasswordGuid).Should().Be(user);
        }

        [Fact]
        public void UserService_GetCurrentUser_HttpContextUserIsNullReturnsNull()
        {
            var httpContextBase = A.Fake<HttpContextBase>();
            A.CallTo(() => httpContextBase.User).Returns(null);

            _userService.GetCurrentUser(httpContextBase).Should().BeNull();
        }

        [Fact]
        public void UserService_GetCurrentUser_HttpContextUserHasIdentityGetByEmail()
        {
            var httpContextBase = A.Fake<HttpContextBase>();
            var principal = A.Fake<IPrincipal>();
            var identity = A.Fake<IIdentity>();
            A.CallTo(() => identity.Name).Returns("test@example.com");
            A.CallTo(() => principal.Identity).Returns(identity);
            A.CallTo(() => httpContextBase.User).Returns(principal);
            var user = new User
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
            };
            _inMemoryRepository.Add(user);

            _userService.GetCurrentUser(httpContextBase).Should().Be(user);
        }

    }
}