namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class LegacyDeductionScoreChecks
{
    public static void LegacyDeductionScoreStartsInCreated()
    {
        var item = LegacyDeductionScore.NewLegacyDeductionScore("code-820", "name-320");
        Check.Equal(item.Status, LegacyDeductionScoreStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyDeductionScoreMovesToValidated()
    {
        var item = LegacyDeductionScore.NewLegacyDeductionScore("code-820", "name-320");
        try
        {
            item.MoveTo(LegacyDeductionScoreStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyDeductionScoreStatus.Validated, "status");
    }

    public static void LegacyDeductionScoreRejectsMovesFromRetired()
    {
        var item = LegacyDeductionScore.NewLegacyDeductionScore("code-820", "name-320");
        item.Status = LegacyDeductionScoreStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyDeductionScoreStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyDeductionScoreStartsInCreated();
        LegacyDeductionScoreMovesToValidated();
        LegacyDeductionScoreRejectsMovesFromRetired();
    }
}
