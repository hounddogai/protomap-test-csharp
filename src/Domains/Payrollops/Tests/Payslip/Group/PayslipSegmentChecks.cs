namespace Acme.Workforce.Payrollops.Tests.Payslip.Group;

using Acme.Workforce.Payrollops.Payslip.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PayslipSegmentChecks
{
    public static void PayslipSegmentStartsInRequested()
    {
        var item = PayslipSegment.NewPayslipSegment("code-948", "title-328");
        Check.Equal(item.Status, PayslipSegmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipSegmentMovesToGranted()
    {
        var item = PayslipSegment.NewPayslipSegment("code-948", "title-328");
        try
        {
            item.MoveTo(PayslipSegmentStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipSegmentStatus.Granted, "status");
    }

    public static void PayslipSegmentRejectsMovesFromExpired()
    {
        var item = PayslipSegment.NewPayslipSegment("code-948", "title-328");
        item.Status = PayslipSegmentStatus.Expired;
        Check.Fails(() => item.MoveTo(PayslipSegmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayslipSegmentStartsInRequested();
        PayslipSegmentMovesToGranted();
        PayslipSegmentRejectsMovesFromExpired();
    }
}
