namespace Acme.Workforce.Payrollops.Tests.PayRun.Preference;

using Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedPayRunPreferenceChecks
{
    public static void WeightedPayRunPreferenceStartsInProposed()
    {
        var item = WeightedPayRunPreference.NewWeightedPayRunPreference("code-676", "title-466");
        Check.Equal(item.Status, WeightedPayRunPreferenceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedPayRunPreferenceMovesToApproved()
    {
        var item = WeightedPayRunPreference.NewWeightedPayRunPreference("code-676", "title-466");
        try
        {
            item.MoveTo(WeightedPayRunPreferenceStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedPayRunPreferenceStatus.Approved, "status");
    }

    public static void WeightedPayRunPreferenceRejectsMovesFromWithdrawn()
    {
        var item = WeightedPayRunPreference.NewWeightedPayRunPreference("code-676", "title-466");
        item.Status = WeightedPayRunPreferenceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedPayRunPreferenceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedPayRunPreferenceStartsInProposed();
        WeightedPayRunPreferenceMovesToApproved();
        WeightedPayRunPreferenceRejectsMovesFromWithdrawn();
    }
}
