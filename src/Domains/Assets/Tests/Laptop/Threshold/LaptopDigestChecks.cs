namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LaptopDigestChecks
{
    public static void LaptopDigestStartsInIdle()
    {
        var item = LaptopDigest.NewLaptopDigest("code-766", "name-618");
        Check.Equal(item.Status, LaptopDigestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopDigestMovesToStopped()
    {
        var item = LaptopDigest.NewLaptopDigest("code-766", "name-618");
        try
        {
            item.MoveTo(LaptopDigestStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopDigestStatus.Stopped, "status");
    }

    public static void LaptopDigestRejectsMovesFromStopped()
    {
        var item = LaptopDigest.NewLaptopDigest("code-766", "name-618");
        item.Status = LaptopDigestStatus.Stopped;
        Check.Fails(() => item.MoveTo(LaptopDigestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LaptopDigestStartsInIdle();
        LaptopDigestMovesToStopped();
        LaptopDigestRejectsMovesFromStopped();
    }
}
