namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanLedgerChecks
{
    public static void SuccessionPlanLedgerStartsInCreated()
    {
        var item = SuccessionPlanLedger.NewSuccessionPlanLedger("code-967", "title-676");
        Check.Equal(item.Status, SuccessionPlanLedgerStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanLedgerMovesToValidated()
    {
        var item = SuccessionPlanLedger.NewSuccessionPlanLedger("code-967", "title-676");
        try
        {
            item.MoveTo(SuccessionPlanLedgerStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanLedgerStatus.Validated, "status");
    }

    public static void SuccessionPlanLedgerRejectsMovesFromRetired()
    {
        var item = SuccessionPlanLedger.NewSuccessionPlanLedger("code-967", "title-676");
        item.Status = SuccessionPlanLedgerStatus.Retired;
        Check.Fails(() => item.MoveTo(SuccessionPlanLedgerStatus.Created), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanLedgerStartsInCreated();
        SuccessionPlanLedgerMovesToValidated();
        SuccessionPlanLedgerRejectsMovesFromRetired();
    }
}
