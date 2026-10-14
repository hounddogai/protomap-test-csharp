namespace Acme.Workforce.Payrollops.Tests.Deduction.Policy;

using Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DeductionScoreChecks
{
    public static void DeductionScoreStartsInCreated()
    {
        var item = DeductionScore.NewDeductionScore("code-484", "title-776");
        Check.Equal(item.Status, DeductionScoreStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionScoreMovesToPublished()
    {
        var item = DeductionScore.NewDeductionScore("code-484", "title-776");
        try
        {
            item.MoveTo(DeductionScoreStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionScoreStatus.Published, "status");
    }

    public static void DeductionScoreRejectsMovesFromRetired()
    {
        var item = DeductionScore.NewDeductionScore("code-484", "title-776");
        item.Status = DeductionScoreStatus.Retired;
        Check.Fails(() => item.MoveTo(DeductionScoreStatus.Created), "move");
    }

    public static void RunAll()
    {
        DeductionScoreStartsInCreated();
        DeductionScoreMovesToPublished();
        DeductionScoreRejectsMovesFromRetired();
    }
}
