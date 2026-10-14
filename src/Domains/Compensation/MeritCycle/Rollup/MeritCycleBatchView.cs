namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle batch.</summary>
public sealed record MeritCycleBatchView(string Code, string LabelText, string ManagerId, string ExternalRef, string ActiveText, string Region, string LocationText, string BatchSize, string Ceiling, string CreatedAtText, string StatusLabel)
{
    public static MeritCycleBatchView ToMeritCycleBatchView(MeritCycleBatch item)
    {
        return new MeritCycleBatchView(
            item.Code,
            Text.Truncate(item.Label, 64),
            Text.Truncate(item.ManagerId, 64),
            item.ExternalRef,
            $"{item.Active}",
            item.Region,
            item.Location,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleBatchView> ToMeritCycleBatchViews(List<MeritCycleBatch> items)
    {
        var views = new List<MeritCycleBatchView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleBatchView(item));
        }
        return views;
    }

    public static double SumMeritCycleBatchView(MeritCycleBatchView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedCeiling + (double)parsedCreatedAt;
    }
}
