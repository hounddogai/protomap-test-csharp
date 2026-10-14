namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global garnishment target.</summary>
public sealed record GlobalGarnishmentTargetView(string Code, string TitleText, string Country, string Margin, string Hours, string Grade, string Active, string Unit, string StatusLabel)
{
    public static GlobalGarnishmentTargetView ToGlobalGarnishmentTargetView(GlobalGarnishmentTarget item)
    {
        return new GlobalGarnishmentTargetView(
            item.Code,
            item.Title,
            Text.Truncate(item.Country, 120),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 64),
            $"{item.Active}",
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalGarnishmentTargetView> ToGlobalGarnishmentTargetViews(List<GlobalGarnishmentTarget> items)
    {
        var views = new List<GlobalGarnishmentTargetView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalGarnishmentTargetView(item));
        }
        return views;
    }

    public static double SumGlobalGarnishmentTargetView(GlobalGarnishmentTargetView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedMargin + parsedHours;
    }
}
