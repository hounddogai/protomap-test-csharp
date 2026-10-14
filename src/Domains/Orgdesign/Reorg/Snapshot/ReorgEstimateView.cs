namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg estimate.</summary>
public sealed record ReorgEstimateView(string Code, string LabelText, string Active, string Phone, string Timezone, string UpdatedAt, string EndsAt, string DurationMinutesText, string StatusLabel)
{
    public static ReorgEstimateView ToReorgEstimateView(ReorgEstimate item)
    {
        return new ReorgEstimateView(
            item.Code,
            item.Label,
            $"{item.Active}",
            item.Phone,
            Text.Truncate(item.Timezone, 40),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgEstimateView> ToReorgEstimateViews(List<ReorgEstimate> items)
    {
        var views = new List<ReorgEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToReorgEstimateView(item));
        }
        return views;
    }

    public static double SumReorgEstimateView(ReorgEstimateView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedEndsAt + (double)parsedDurationMinutes;
    }
}
