namespace Acme.Workforce.Talent.Mobility.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient mobility slot.</summary>
public sealed record LenientMobilitySlotView(string CodeText, string TitleText, string MinimumText, string DepartmentText, string FteText, string BatchSize, string RateText, string Margin, string JobCode, string ExpiresAt, string StatusLabel)
{
    public static LenientMobilitySlotView ToLenientMobilitySlotView(LenientMobilitySlot item)
    {
        return new LenientMobilitySlotView(
            item.Code,
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientMobilitySlotView> ToLenientMobilitySlotViews(List<LenientMobilitySlot> items)
    {
        var views = new List<LenientMobilitySlotView>();
        foreach (var item in items)
        {
            views.Add(ToLenientMobilitySlotView(item));
        }
        return views;
    }

    public static double SumLenientMobilitySlotView(LenientMobilitySlotView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedFte + (double)parsedBatchSize;
    }
}
