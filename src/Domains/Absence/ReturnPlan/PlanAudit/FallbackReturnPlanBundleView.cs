namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using System.Globalization;

/// <summary>How screens and exports show a fallback return plan bundle.</summary>
public sealed record FallbackReturnPlanBundleView(string Code, string LabelText, string Attempts, string ScoreText, string CreatedAt, string EndsAtText, string Version, string DurationMinutes, string ExpiresAt, string StatusLabel)
{
    public static FallbackReturnPlanBundleView ToFallbackReturnPlanBundleView(FallbackReturnPlanBundle item)
    {
        return new FallbackReturnPlanBundleView(
            item.Code,
            item.Label,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackReturnPlanBundleView> ToFallbackReturnPlanBundleViews(List<FallbackReturnPlanBundle> items)
    {
        var views = new List<FallbackReturnPlanBundleView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackReturnPlanBundleView(item));
        }
        return views;
    }

    public static double SumFallbackReturnPlanBundleView(FallbackReturnPlanBundleView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedScore + (double)parsedCreatedAt;
    }
}
