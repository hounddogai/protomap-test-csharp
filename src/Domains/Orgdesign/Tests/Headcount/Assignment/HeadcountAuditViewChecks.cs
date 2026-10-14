namespace Acme.Workforce.Orgdesign.Tests.Headcount.Assignment;

using Acme.Workforce.Orgdesign.Headcount.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class HeadcountAuditViewChecks
{
    public static void HeadcountAuditViewShowsTheStatusInCapitals()
    {
        var item = HeadcountAudit.NewHeadcountAudit("code-764", "label-232");
        var view = HeadcountAuditView.ToHeadcountAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountAuditView.ToHeadcountAuditViews(new List<HeadcountAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountAuditViewShowsTheStatusInCapitals();
    }
}
