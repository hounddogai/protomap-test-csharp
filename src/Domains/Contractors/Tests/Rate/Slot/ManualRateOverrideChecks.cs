namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ManualRateOverrideChecks
{
    public static void ManualRateOverrideStartsInCreated()
    {
        var item = ManualRateOverride.NewManualRateOverride("code-391", "name-173");
        Check.Equal(item.Status, ManualRateOverrideStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualRateOverrideMovesToRetired()
    {
        var item = ManualRateOverride.NewManualRateOverride("code-391", "name-173");
        try
        {
            item.MoveTo(ManualRateOverrideStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualRateOverrideStatus.Retired, "status");
    }

    public static void ManualRateOverrideRejectsMovesFromRetired()
    {
        var item = ManualRateOverride.NewManualRateOverride("code-391", "name-173");
        item.Status = ManualRateOverrideStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualRateOverrideStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualRateOverrideStartsInCreated();
        ManualRateOverrideMovesToRetired();
        ManualRateOverrideRejectsMovesFromRetired();
    }
}
