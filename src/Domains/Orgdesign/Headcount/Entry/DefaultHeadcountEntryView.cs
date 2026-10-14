namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default headcount entry.</summary>
public sealed record DefaultHeadcountEntryView(string Code, string LabelText, string Checksum, string FloorValueText, string Weight, string StatusLabel)
{
    public static DefaultHeadcountEntryView ToDefaultHeadcountEntryView(DefaultHeadcountEntry item)
    {
        return new DefaultHeadcountEntryView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Checksum,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultHeadcountEntryView> ToDefaultHeadcountEntryViews(List<DefaultHeadcountEntry> items)
    {
        var views = new List<DefaultHeadcountEntryView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultHeadcountEntryView(item));
        }
        return views;
    }

    public static double SumDefaultHeadcountEntryView(DefaultHeadcountEntryView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedWeight;
    }
}
