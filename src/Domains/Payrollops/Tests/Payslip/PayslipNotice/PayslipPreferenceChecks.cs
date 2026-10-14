namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class PayslipPreferenceChecks
{
    public static void PayslipPreferenceStartsInRequested()
    {
        var item = PayslipPreference.NewPayslipPreference("code-677", "name-580");
        Check.Equal(item.Status, PayslipPreferenceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipPreferenceMovesToGranted()
    {
        var item = PayslipPreference.NewPayslipPreference("code-677", "name-580");
        try
        {
            item.MoveTo(PayslipPreferenceStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipPreferenceStatus.Granted, "status");
    }

    public static void PayslipPreferenceRejectsMovesFromExpired()
    {
        var item = PayslipPreference.NewPayslipPreference("code-677", "name-580");
        item.Status = PayslipPreferenceStatus.Expired;
        Check.Fails(() => item.MoveTo(PayslipPreferenceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayslipPreferenceStartsInRequested();
        PayslipPreferenceMovesToGranted();
        PayslipPreferenceRejectsMovesFromExpired();
    }
}
