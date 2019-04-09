using System;
using System.Threading.Tasks;
using Moq;
using Tinkoff.ISA.AppLayer.Slack;
using Tinkoff.ISA.AppLayer.Slack.Executing;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack
{
    public class ActionExecutorServiceTests
    {
        private readonly ISlackExecutorService _service;
        private readonly Mock<IServiceProvider> _serviceProviderMock;

        public ActionExecutorServiceTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _service = new SlackExecutorService(_serviceProviderMock.Object);
        }


        private class Foo : ISlackActionHandler<Bar>
        {
            public bool WasInvoked { get; private set; }

            public Task Handle(Bar actionParams)
            {
                WasInvoked = true;
                return Task.CompletedTask;
            }
        }

        private class Bar : ISlackActionParams
        {
        }

        [Fact]
        public async Task Execute_JustInvoked_ShouldInvokeHandle()
        {
            // Arrange
            var testHandler = new Foo();
            var actionParams = new Bar();
            _serviceProviderMock
                .Setup(m => m.GetService(typeof(ISlackActionHandler<Bar>)))
                .Returns(testHandler);

            // Act
            await _service.ExecuteAction(actionParams.GetType(), actionParams);

            // Assert
            _serviceProviderMock.Verify(m => m.GetService(typeof(ISlackActionHandler<Bar>)), Times.Once);
            _serviceProviderMock.VerifyNoOtherCalls();
            Assert.True(testHandler.WasInvoked);
        }

        [Fact]
        public async Task Execute_ParamsTypeIsNull_ArgumentNullException()
        {
            // Act, Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ExecuteAction(null));
        }

        [Fact]
        public async Task Execute_NoPassedParams_ArgumentException()
        {
            // Act, Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExecuteAction(typeof(Bar)));
        }
    }
}
