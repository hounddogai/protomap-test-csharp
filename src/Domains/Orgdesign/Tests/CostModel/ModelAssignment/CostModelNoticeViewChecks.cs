namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelNoticeViewChecks
{
    public static void CostModelNoticeViewShowsTheStatusInCapitals()
    {
        var item = CostModelNotice.NewCostModelNotice("code-465", "title-703");
        var view = CostModelNoticeView.ToCostModelNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CostModelNoticeView.ToCostModelNoticeViews(new List<CostModelNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CostModelNoticeViewShowsTheStatusInCapitals();
    }
}
