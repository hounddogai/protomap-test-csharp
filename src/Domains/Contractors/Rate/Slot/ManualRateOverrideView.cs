namespace Acme.Workforce.Contractors.Rate.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual rate override.</summary>
public sealed record ManualRateOverrideView(string Code, string Name, string Approved, string ExternalRef, string SequenceText, string Ratio, string Locale, string GraceDays, string Score, string StatusLabel)
{
    public static ManualRateOverrideView ToManualRateOverrideView(ManualRateOverride item)
    {
        return new ManualRateOverrideView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 64),
            $"{item.Approved}",
            item.ExternalRef,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 80),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualRateOverrideView> ToManualRateOverrideViews(List<ManualRateOverride> items)
    {
        var views = new List<ManualRateOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToManualRateOverrideView(item));
        }
        return views;
    }

    public static double SumManualRateOverrideView(ManualRateOverrideView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedRatio + (double)parsedGraceDays;
    }
}
