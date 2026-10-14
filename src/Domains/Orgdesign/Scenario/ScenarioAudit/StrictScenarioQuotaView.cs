namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict scenario quota.</summary>
public sealed record StrictScenarioQuotaView(string CodeText, string NameText, string StartsAt, string CreatedAtText, string DurationMinutes, string Sequence, string Country, string Segment, string Pinned, string ActiveText, string StatusLabel)
{
    public static StrictScenarioQuotaView ToStrictScenarioQuotaView(StrictScenarioQuota item)
    {
        return new StrictScenarioQuotaView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 40),
            item.Segment,
            $"{item.Pinned}",
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictScenarioQuotaView> ToStrictScenarioQuotaViews(List<StrictScenarioQuota> items)
    {
        var views = new List<StrictScenarioQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToStrictScenarioQuotaView(item));
        }
        return views;
    }

    public static double SumStrictScenarioQuotaView(StrictScenarioQuotaView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedCreatedAt + (double)parsedDurationMinutes;
    }
}
