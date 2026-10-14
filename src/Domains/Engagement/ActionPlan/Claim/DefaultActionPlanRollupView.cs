namespace Acme.Workforce.Engagement.ActionPlan.Claim;

using System.Globalization;

/// <summary>How screens and exports show a default action plan rollup.</summary>
public sealed record DefaultActionPlanRollupView(string CodeText, string LabelText, string Rate, string ExpiresAtText, string JobCodeText, string StartsAtText, string DurationMinutesText, string Revision, string TeamText, string StatusLabel)
{
    public static DefaultActionPlanRollupView ToDefaultActionPlanRollupView(DefaultActionPlanRollup item)
    {
        return new DefaultActionPlanRollupView(
            item.Code,
            item.Label,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultActionPlanRollupView> ToDefaultActionPlanRollupViews(List<DefaultActionPlanRollup> items)
    {
        var views = new List<DefaultActionPlanRollupView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultActionPlanRollupView(item));
        }
        return views;
    }

    public static double SumDefaultActionPlanRollupView(DefaultActionPlanRollupView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedExpiresAt + (double)parsedStartsAt;
    }
}
