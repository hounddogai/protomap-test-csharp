namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RetroPayRecordViewChecks
{
    public static void RetroPayRecordViewShowsTheStatusInCapitals()
    {
        var item = RetroPayRecord.NewRetroPayRecord("code-873", "name-777");
        var view = RetroPayRecordView.ToRetroPayRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayRecordView.ToRetroPayRecordViews(new List<RetroPayRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayRecordViewShowsTheStatusInCapitals();
    }
}
