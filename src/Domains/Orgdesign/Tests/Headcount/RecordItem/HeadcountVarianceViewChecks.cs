namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountVarianceViewChecks
{
    public static void HeadcountVarianceViewShowsTheStatusInCapitals()
    {
        var item = HeadcountVariance.NewHeadcountVariance("code-765", "label-685");
        var view = HeadcountVarianceView.ToHeadcountVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountVarianceView.ToHeadcountVarianceViews(new List<HeadcountVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountVarianceViewShowsTheStatusInCapitals();
    }
}
