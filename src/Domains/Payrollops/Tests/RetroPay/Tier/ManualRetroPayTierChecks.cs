namespace Acme.Workforce.Payrollops.Tests.RetroPay.Tier;

using Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualRetroPayTierChecks
{
    public static void ManualRetroPayTierStartsInOpen()
    {
        var item = ManualRetroPayTier.NewManualRetroPayTier("code-434", "name-959");
        Check.Equal(item.Status, ManualRetroPayTierStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualRetroPayTierMovesToResolved()
    {
        var item = ManualRetroPayTier.NewManualRetroPayTier("code-434", "name-959");
        try
        {
            item.MoveTo(ManualRetroPayTierStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualRetroPayTierStatus.Resolved, "status");
    }

    public static void ManualRetroPayTierRejectsMovesFromArchived()
    {
        var item = ManualRetroPayTier.NewManualRetroPayTier("code-434", "name-959");
        item.Status = ManualRetroPayTierStatus.Archived;
        Check.Fails(() => item.MoveTo(ManualRetroPayTierStatus.Open), "move");
    }

    public static void RunAll()
    {
        ManualRetroPayTierStartsInOpen();
        ManualRetroPayTierMovesToResolved();
        ManualRetroPayTierRejectsMovesFromArchived();
    }
}
