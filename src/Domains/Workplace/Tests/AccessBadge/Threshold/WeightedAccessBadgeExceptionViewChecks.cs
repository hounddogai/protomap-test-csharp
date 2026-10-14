namespace Acme.Workforce.Workplace.Tests.AccessBadge.Threshold;

using Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class WeightedAccessBadgeExceptionViewChecks
{
    public static void WeightedAccessBadgeExceptionViewShowsTheStatusInCapitals()
    {
        var item = WeightedAccessBadgeException.NewWeightedAccessBadgeException("code-345", "label-357");
        var view = WeightedAccessBadgeExceptionView.ToWeightedAccessBadgeExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedAccessBadgeExceptionView.ToWeightedAccessBadgeExceptionViews(new List<WeightedAccessBadgeException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeExceptionViewShowsTheStatusInCapitals();
    }
}
