namespace Acme.Workforce.Orgdesign.Headcount.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount bundle.</summary>
public sealed record HeadcountBundleView(string CodeText, string Label, string CeilingText, string GraceDaysText, string CreatedAtText, string Rating, string StatusLabel)
{
    public static HeadcountBundleView ToHeadcountBundleView(HeadcountBundle item)
    {
        return new HeadcountBundleView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 64),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountBundleView> ToHeadcountBundleViews(List<HeadcountBundle> items)
    {
        var views = new List<HeadcountBundleView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountBundleView(item));
        }
        return views;
    }

    public static double SumHeadcountBundleView(HeadcountBundleView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedGraceDays + (double)parsedCreatedAt;
    }
}
