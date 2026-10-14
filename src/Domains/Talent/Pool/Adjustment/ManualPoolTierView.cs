namespace Acme.Workforce.Talent.Pool.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a manual pool tier.</summary>
public sealed record ManualPoolTierView(string Code, string Name, string Segment, string DescriptionText, string Maximum, string VisibleText, string StatusLabel)
{
    public static ManualPoolTierView ToManualPoolTierView(ManualPoolTier item)
    {
        return new ManualPoolTierView(
            item.Code,
            item.Name,
            item.Segment,
            item.Description,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualPoolTierView> ToManualPoolTierViews(List<ManualPoolTier> items)
    {
        var views = new List<ManualPoolTierView>();
        foreach (var item in items)
        {
            views.Add(ToManualPoolTierView(item));
        }
        return views;
    }

    public static double SumManualPoolTierView(ManualPoolTierView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
