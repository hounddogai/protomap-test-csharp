namespace Acme.Workforce.Compensation.Adjustment.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy adjustment batch.</summary>
public sealed record LegacyAdjustmentBatchView(string Code, string Title, string Team, string UnitText, string Description, string Location, string SummaryText, string CeilingText, string StatusLabel)
{
    public static LegacyAdjustmentBatchView ToLegacyAdjustmentBatchView(LegacyAdjustmentBatch item)
    {
        return new LegacyAdjustmentBatchView(
            item.Code,
            item.Title,
            Text.Truncate(item.Team, 80),
            item.Unit,
            item.Description,
            item.Location,
            Text.Truncate(item.Summary, 120),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyAdjustmentBatchView> ToLegacyAdjustmentBatchViews(List<LegacyAdjustmentBatch> items)
    {
        var views = new List<LegacyAdjustmentBatchView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyAdjustmentBatchView(item));
        }
        return views;
    }

    public static double SumLegacyAdjustmentBatchView(LegacyAdjustmentBatchView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedCeiling;
    }
}
