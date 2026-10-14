namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunExceptionChecks
{
    public static void PayRunExceptionStartsInCreated()
    {
        var item = PayRunException.NewPayRunException("code-314", "label-372");
        Check.Equal(item.Status, PayRunExceptionStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunExceptionMovesToValidated()
    {
        var item = PayRunException.NewPayRunException("code-314", "label-372");
        try
        {
            item.MoveTo(PayRunExceptionStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunExceptionStatus.Validated, "status");
    }

    public static void PayRunExceptionRejectsMovesFromRetired()
    {
        var item = PayRunException.NewPayRunException("code-314", "label-372");
        item.Status = PayRunExceptionStatus.Retired;
        Check.Fails(() => item.MoveTo(PayRunExceptionStatus.Created), "move");
    }

    public static void RunAll()
    {
        PayRunExceptionStartsInCreated();
        PayRunExceptionMovesToValidated();
        PayRunExceptionRejectsMovesFromRetired();
    }
}
