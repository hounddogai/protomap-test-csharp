namespace Acme.Workforce.Absence.ReturnPlan.Summary;

using System.Globalization;

/// <summary>How screens and exports show a lenient return plan override.</summary>
public sealed record LenientReturnPlanOverrideView(string Code, string Name, string Retries, string Source, string Score, string Title, string StatusLabel)
{
    public static LenientReturnPlanOverrideView ToLenientReturnPlanOverrideView(LenientReturnPlanOverride item)
    {
        return new LenientReturnPlanOverrideView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientReturnPlanOverrideView> ToLenientReturnPlanOverrideViews(List<LenientReturnPlanOverride> items)
    {
        var views = new List<LenientReturnPlanOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToLenientReturnPlanOverrideView(item));
        }
        return views;
    }

    public static double SumLenientReturnPlanOverrideView(LenientReturnPlanOverrideView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedScore;
    }
}
