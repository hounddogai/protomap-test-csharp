namespace Acme.Workforce.Talent.Nomination.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered nomination rule.</summary>
public sealed record TieredNominationRuleView(string CodeText, string NameText, string Unit, string SalaryBandText, string CreatedAt, string FloorValue, string Locale, string Archived, string MaximumText, string StatusLabel)
{
    public static TieredNominationRuleView ToTieredNominationRuleView(TieredNominationRule item)
    {
        return new TieredNominationRuleView(
            item.Code,
            item.Name,
            Text.Truncate(item.Unit, 40),
            item.SalaryBand,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Archived}",
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredNominationRuleView> ToTieredNominationRuleViews(List<TieredNominationRule> items)
    {
        var views = new List<TieredNominationRuleView>();
        foreach (var item in items)
        {
            views.Add(ToTieredNominationRuleView(item));
        }
        return views;
    }

    public static double SumTieredNominationRuleView(TieredNominationRuleView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedFloorValue + parsedMaximum;
    }
}
