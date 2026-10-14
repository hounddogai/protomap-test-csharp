namespace Acme.Workforce.Workplace.Visitor.Score;

using System.Globalization;

/// <summary>How screens and exports show a adaptive visitor override.</summary>
public sealed record AdaptiveVisitorOverrideView(string Code, string NameText, string MarginText, string CategoryText, string Notes, string ExternalRefText, string Ceiling, string ChecksumText, string BatchSize, string Currency, string SegmentText, string StatusLabel)
{
    public static AdaptiveVisitorOverrideView ToAdaptiveVisitorOverrideView(AdaptiveVisitorOverride item)
    {
        return new AdaptiveVisitorOverrideView(
            item.Code,
            item.Name,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.Notes,
            item.ExternalRef,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveVisitorOverrideView> ToAdaptiveVisitorOverrideViews(List<AdaptiveVisitorOverride> items)
    {
        var views = new List<AdaptiveVisitorOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveVisitorOverrideView(item));
        }
        return views;
    }

    public static double SumAdaptiveVisitorOverrideView(AdaptiveVisitorOverrideView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedMargin + parsedCeiling + (double)parsedBatchSize;
    }
}
