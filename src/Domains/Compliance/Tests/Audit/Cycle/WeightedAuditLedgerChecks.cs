namespace Acme.Workforce.Compliance.Tests.Audit.Cycle;

using Acme.Workforce.Compliance.Audit.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class WeightedAuditLedgerChecks
{
    public static void WeightedAuditLedgerStartsInCreated()
    {
        var item = WeightedAuditLedger.NewWeightedAuditLedger("code-732", "label-640");
        Check.Equal(item.Status, WeightedAuditLedgerStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAuditLedgerMovesToValidated()
    {
        var item = WeightedAuditLedger.NewWeightedAuditLedger("code-732", "label-640");
        try
        {
            item.MoveTo(WeightedAuditLedgerStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAuditLedgerStatus.Validated, "status");
    }

    public static void WeightedAuditLedgerRejectsMovesFromRetired()
    {
        var item = WeightedAuditLedger.NewWeightedAuditLedger("code-732", "label-640");
        item.Status = WeightedAuditLedgerStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedAuditLedgerStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedAuditLedgerStartsInCreated();
        WeightedAuditLedgerMovesToValidated();
        WeightedAuditLedgerRejectsMovesFromRetired();
    }
}
