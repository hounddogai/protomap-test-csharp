namespace Acme.Workforce.Contractors.Engagement.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement cycle.</summary>
public sealed record EngagementCycleView(string Code, string LabelText, string EndsAt, string Grade, string Retries, string StartsAt, string ContactEmail, string Ratio, string CreatedAtText, string FloorValue, string StatusLabel)
{
    public static EngagementCycleView ToEngagementCycleView(EngagementCycle item)
    {
        return new EngagementCycleView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementCycleView> ToEngagementCycleViews(List<EngagementCycle> items)
    {
        var views = new List<EngagementCycleView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementCycleView(item));
        }
        return views;
    }

    public static double SumEngagementCycleView(EngagementCycleView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedRetries + (double)parsedStartsAt;
    }
}
