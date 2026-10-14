namespace Acme.Workforce.Compensation.BonusPool.Entry;

using System.Globalization;

/// <summary>How screens and exports show a bonus pool segment.</summary>
public sealed record BonusPoolSegmentView(string Code, string LabelText, string PriorityText, string Maximum, string Country, string Fte, string Version, string BatchSize, string StatusLabel)
{
    public static BonusPoolSegmentView ToBonusPoolSegmentView(BonusPoolSegment item)
    {
        return new BonusPoolSegmentView(
            item.Code,
            item.Label,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolSegmentView> ToBonusPoolSegmentViews(List<BonusPoolSegment> items)
    {
        var views = new List<BonusPoolSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolSegmentView(item));
        }
        return views;
    }

    public static double SumBonusPoolSegmentView(BonusPoolSegmentView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedMaximum + parsedFte;
    }
}
