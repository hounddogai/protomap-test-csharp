namespace Acme.Workforce.Orgdesign.Tests.Position.Quota;

using Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class GlobalPositionVarianceViewChecks
{
    public static void GlobalPositionVarianceViewShowsTheStatusInCapitals()
    {
        var item = GlobalPositionVariance.NewGlobalPositionVariance("code-155", "title-406");
        var view = GlobalPositionVarianceView.ToGlobalPositionVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalPositionVarianceView.ToGlobalPositionVarianceViews(new List<GlobalPositionVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalPositionVarianceViewShowsTheStatusInCapitals();
    }
}
