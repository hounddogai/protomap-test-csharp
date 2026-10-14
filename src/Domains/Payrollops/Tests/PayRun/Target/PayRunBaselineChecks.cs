namespace Acme.Workforce.Payrollops.Tests.PayRun.Target;

using Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PayRunBaselineChecks
{
    public static void PayRunBaselineStartsInCreated()
    {
        var item = PayRunBaseline.NewPayRunBaseline("code-860", "title-972");
        Check.Equal(item.Status, PayRunBaselineStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunBaselineMovesToValidated()
    {
        var item = PayRunBaseline.NewPayRunBaseline("code-860", "title-972");
        try
        {
            item.MoveTo(PayRunBaselineStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunBaselineStatus.Validated, "status");
    }

    public static void PayRunBaselineRejectsMovesFromRetired()
    {
        var item = PayRunBaseline.NewPayRunBaseline("code-860", "title-972");
        item.Status = PayRunBaselineStatus.Retired;
        Check.Fails(() => item.MoveTo(PayRunBaselineStatus.Created), "move");
    }

    public static void RunAll()
    {
        PayRunBaselineStartsInCreated();
        PayRunBaselineMovesToValidated();
        PayRunBaselineRejectsMovesFromRetired();
    }
}
