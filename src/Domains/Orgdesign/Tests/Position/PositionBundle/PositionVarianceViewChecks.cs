namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class PositionVarianceViewChecks
{
    public static void PositionVarianceViewShowsTheStatusInCapitals()
    {
        var item = PositionVariance.NewPositionVariance("code-991", "label-899");
        var view = PositionVarianceView.ToPositionVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionVarianceView.ToPositionVarianceViews(new List<PositionVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionVarianceViewShowsTheStatusInCapitals();
    }
}
