namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount override.</summary>
public sealed record HeadcountOverrideView(string CodeText, string Label, string Country, string MinimumText, string Limit, string DescriptionText, string CeilingText, string ArchivedText, string Rate, string ExpiresAt, string StatusLabel)
{
    public static HeadcountOverrideView ToHeadcountOverrideView(HeadcountOverride item)
    {
        return new HeadcountOverrideView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.Country, 64),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountOverrideView> ToHeadcountOverrideViews(List<HeadcountOverride> items)
    {
        var views = new List<HeadcountOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountOverrideView(item));
        }
        return views;
    }

    public static double SumHeadcountOverrideView(HeadcountOverrideView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedLimit + parsedCeiling;
    }
}
