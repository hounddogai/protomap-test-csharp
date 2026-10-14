namespace Acme.Workforce.Talent.Mobility.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility checkpoint.</summary>
public sealed record MobilityCheckpointView(string CodeText, string Name, string MarginText, string LeaveDays, string Label, string Category, string Maximum, string CurrencyText, string StatusLabel)
{
    public static MobilityCheckpointView ToMobilityCheckpointView(MobilityCheckpoint item)
    {
        return new MobilityCheckpointView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Label,
            Text.Truncate(item.Category, 40),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityCheckpointView> ToMobilityCheckpointViews(List<MobilityCheckpoint> items)
    {
        var views = new List<MobilityCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityCheckpointView(item));
        }
        return views;
    }

    public static double SumMobilityCheckpointView(MobilityCheckpointView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedLeaveDays + parsedMaximum;
    }
}
