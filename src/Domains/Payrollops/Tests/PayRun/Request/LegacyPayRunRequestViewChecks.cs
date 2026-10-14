namespace Acme.Workforce.Payrollops.Tests.PayRun.Request;

using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyPayRunRequestViewChecks
{
    public static void LegacyPayRunRequestViewShowsTheStatusInCapitals()
    {
        var item = LegacyPayRunRequest.NewLegacyPayRunRequest("code-564", "title-577");
        var view = LegacyPayRunRequestView.ToLegacyPayRunRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPayRunRequestView.ToLegacyPayRunRequestViews(new List<LegacyPayRunRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPayRunRequestViewShowsTheStatusInCapitals();
    }
}
