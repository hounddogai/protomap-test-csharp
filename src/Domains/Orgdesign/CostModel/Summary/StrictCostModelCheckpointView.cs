namespace Acme.Workforce.Orgdesign.CostModel.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict cost model checkpoint.</summary>
public sealed record StrictCostModelCheckpointView(string CodeText, string Label, string Minimum, string Rate, string TenureMonths, string Unit, string Ceiling, string Location, string SequenceText, string Channel, string Visible, string StatusLabel)
{
    public static StrictCostModelCheckpointView ToStrictCostModelCheckpointView(StrictCostModelCheckpoint item)
    {
        return new StrictCostModelCheckpointView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictCostModelCheckpointView> ToStrictCostModelCheckpointViews(List<StrictCostModelCheckpoint> items)
    {
        var views = new List<StrictCostModelCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToStrictCostModelCheckpointView(item));
        }
        return views;
    }

    public static double SumStrictCostModelCheckpointView(StrictCostModelCheckpointView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedRate + (double)parsedTenureMonths;
    }
}
