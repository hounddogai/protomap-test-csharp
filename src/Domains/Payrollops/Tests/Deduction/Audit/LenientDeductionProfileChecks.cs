namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LenientDeductionProfileChecks
{
    public static void LenientDeductionProfileStartsInDraft()
    {
        var item = LenientDeductionProfile.NewLenientDeductionProfile("code-942", "name-331");
        Check.Equal(item.Status, LenientDeductionProfileStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientDeductionProfileMovesToClosed()
    {
        var item = LenientDeductionProfile.NewLenientDeductionProfile("code-942", "name-331");
        try
        {
            item.MoveTo(LenientDeductionProfileStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientDeductionProfileStatus.Closed, "status");
    }

    public static void LenientDeductionProfileRejectsMovesFromClosed()
    {
        var item = LenientDeductionProfile.NewLenientDeductionProfile("code-942", "name-331");
        item.Status = LenientDeductionProfileStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientDeductionProfileStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientDeductionProfileStartsInDraft();
        LenientDeductionProfileMovesToClosed();
        LenientDeductionProfileRejectsMovesFromClosed();
    }
}
