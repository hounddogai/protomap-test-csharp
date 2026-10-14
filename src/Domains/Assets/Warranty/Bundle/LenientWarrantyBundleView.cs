namespace Acme.Workforce.Assets.Warranty.Bundle;

using System.Globalization;

/// <summary>How screens and exports show a lenient warranty bundle.</summary>
public sealed record LenientWarrantyBundleView(string Code, string LabelText, string Skill, string Priority, string TenureMonths, string StatusLabel)
{
    public static LenientWarrantyBundleView ToLenientWarrantyBundleView(LenientWarrantyBundle item)
    {
        return new LenientWarrantyBundleView(
            item.Code,
            item.Label,
            item.Skill,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientWarrantyBundleView> ToLenientWarrantyBundleViews(List<LenientWarrantyBundle> items)
    {
        var views = new List<LenientWarrantyBundleView>();
        foreach (var item in items)
        {
            views.Add(ToLenientWarrantyBundleView(item));
        }
        return views;
    }

    public static double SumLenientWarrantyBundleView(LenientWarrantyBundleView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedTenureMonths;
    }
}
