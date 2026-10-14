namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ExceptionQuotaViewChecks
{
    public static void ExceptionQuotaViewShowsTheStatusInCapitals()
    {
        var item = ExceptionQuota.NewExceptionQuota("code-670", "label-269");
        var view = ExceptionQuotaView.ToExceptionQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionQuotaView.ToExceptionQuotaViews(new List<ExceptionQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionQuotaViewShowsTheStatusInCapitals();
    }
}
