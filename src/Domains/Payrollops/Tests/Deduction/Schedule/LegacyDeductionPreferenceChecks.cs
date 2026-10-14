namespace Acme.Workforce.Payrollops.Tests.Deduction.Schedule;

using Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LegacyDeductionPreferenceChecks
{
    public static void LegacyDeductionPreferenceStartsInDraft()
    {
        var item = LegacyDeductionPreference.NewLegacyDeductionPreference("code-315", "name-270");
        Check.Equal(item.Status, LegacyDeductionPreferenceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyDeductionPreferenceMovesToPending()
    {
        var item = LegacyDeductionPreference.NewLegacyDeductionPreference("code-315", "name-270");
        try
        {
            item.MoveTo(LegacyDeductionPreferenceStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyDeductionPreferenceStatus.Pending, "status");
    }

    public static void LegacyDeductionPreferenceRejectsMovesFromClosed()
    {
        var item = LegacyDeductionPreference.NewLegacyDeductionPreference("code-315", "name-270");
        item.Status = LegacyDeductionPreferenceStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyDeductionPreferenceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyDeductionPreferenceStartsInDraft();
        LegacyDeductionPreferenceMovesToPending();
        LegacyDeductionPreferenceRejectsMovesFromClosed();
    }
}
