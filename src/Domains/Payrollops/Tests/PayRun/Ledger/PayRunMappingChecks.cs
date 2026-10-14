namespace Acme.Workforce.Payrollops.Tests.PayRun.Ledger;

using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PayRunMappingChecks
{
    public static void PayRunMappingStartsInCreated()
    {
        var item = PayRunMapping.NewPayRunMapping("code-153", "name-757");
        Check.Equal(item.Status, PayRunMappingStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunMappingMovesToRetired()
    {
        var item = PayRunMapping.NewPayRunMapping("code-153", "name-757");
        try
        {
            item.MoveTo(PayRunMappingStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunMappingStatus.Retired, "status");
    }

    public static void PayRunMappingRejectsMovesFromRetired()
    {
        var item = PayRunMapping.NewPayRunMapping("code-153", "name-757");
        item.Status = PayRunMappingStatus.Retired;
        Check.Fails(() => item.MoveTo(PayRunMappingStatus.Created), "move");
    }

    public static void RunAll()
    {
        PayRunMappingStartsInCreated();
        PayRunMappingMovesToRetired();
        PayRunMappingRejectsMovesFromRetired();
    }
}
