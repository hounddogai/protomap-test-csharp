namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict tax form tier.</summary>
public sealed record StrictTaxFormTierView(string Code, string Label, string Timezone, string Quantity, string Skill, string VersionText, string StatusLabel)
{
    public static StrictTaxFormTierView ToStrictTaxFormTierView(StrictTaxFormTier item)
    {
        return new StrictTaxFormTierView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Timezone,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 40),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictTaxFormTierView> ToStrictTaxFormTierViews(List<StrictTaxFormTier> items)
    {
        var views = new List<StrictTaxFormTierView>();
        foreach (var item in items)
        {
            views.Add(ToStrictTaxFormTierView(item));
        }
        return views;
    }

    public static double SumStrictTaxFormTierView(StrictTaxFormTierView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedVersion;
    }
}
