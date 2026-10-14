namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ReorgThresholdChecks
{
    public static void ReorgThresholdStartsInCreated()
    {
        var item = ReorgThreshold.NewReorgThreshold("code-85", "title-901");
        Check.Equal(item.Status, ReorgThresholdStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgThresholdMovesToRetired()
    {
        var item = ReorgThreshold.NewReorgThreshold("code-85", "title-901");
        try
        {
            item.MoveTo(ReorgThresholdStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgThresholdStatus.Retired, "status");
    }

    public static void ReorgThresholdRejectsMovesFromRetired()
    {
        var item = ReorgThreshold.NewReorgThreshold("code-85", "title-901");
        item.Status = ReorgThresholdStatus.Retired;
        Check.Fails(() => item.MoveTo(ReorgThresholdStatus.Created), "move");
    }

    public static void RunAll()
    {
        ReorgThresholdStartsInCreated();
        ReorgThresholdMovesToRetired();
        ReorgThresholdRejectsMovesFromRetired();
    }
}
