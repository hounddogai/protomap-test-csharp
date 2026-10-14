namespace Acme.Workforce.Engagement.TownHall.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy town hall schedule.</summary>
public sealed record LegacyTownHallScheduleView(string Code, string Title, string Ceiling, string BatchSizeText, string Pinned, string RevisionText, string Currency, string SourceText, string CapacityText, string ThresholdText, string Amount, string StatusLabel)
{
    public static LegacyTownHallScheduleView ToLegacyTownHallScheduleView(LegacyTownHallSchedule item)
    {
        return new LegacyTownHallScheduleView(
            item.Code,
            item.Title,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 64),
            item.Source,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTownHallScheduleView> ToLegacyTownHallScheduleViews(List<LegacyTownHallSchedule> items)
    {
        var views = new List<LegacyTownHallScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTownHallScheduleView(item));
        }
        return views;
    }

    public static double SumLegacyTownHallScheduleView(LegacyTownHallScheduleView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedBatchSize + (double)parsedRevision;
    }
}
