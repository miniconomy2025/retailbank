using Xunit;
using Microsoft.Extensions.Options;
using RetailBank.Models.Options;
using RetailBank.Services;
using Moq;

namespace RetailBank.Tests;

public class SimulationControllerServiceTest
{
    private readonly Mock<IOptions<SimulationOptions>> _optionsMock;
    private readonly SimulationControllerService _service;

    public SimulationControllerServiceTest()
    {
        _optionsMock = new Mock<IOptions<SimulationOptions>>();
        _optionsMock.Setup(o => o.Value).Returns(new SimulationOptions 
        { 
            TimeScale = 10, 
            SimulationStart = 1000 
        });
        _service = new SimulationControllerService(_optionsMock.Object);
    }

    [Fact]
    public void Constructor_InitialState_IsNotRunning()
    {
        Assert.False(_service.IsRunning);
    }

    [Fact]
    public void Start_SetsIsRunningTrue()
    {
        _service.Start(5000);
        Assert.True(_service.IsRunning);
    }

    [Fact]
    public void Start_SetsUnixStartTime()
    {
        ulong startTime = 5000;
        _service.Start(startTime);
        Assert.Equal(startTime, _service.UnixStartTime);
    }

    [Fact]
    public void Stop_SetsIsRunningFalse()
    {
        _service.Start(5000);
        Assert.True(_service.IsRunning);

        _service.Stop();
        Assert.False(_service.IsRunning);
    }

    [Fact]
    public void TimeScale_IsConfiguredValue()
    {
        Assert.Equal(10u, _service.TimeScale);
    }

    [Theory]
    [InlineData(5000, 5100, 2000ul)]
    [InlineData(0, 50, 1500ul)]
    [InlineData(1000000, 2000000, 10001000ul)]
    public void TimestampToSim_CalculatesCorrectly(ulong unixStartTime, ulong timestamp, ulong expected)
    {
        _service.Start(unixStartTime);
        ulong result = _service.TimestampToSim(timestamp);
        Assert.Equal(expected, result);
    }
}