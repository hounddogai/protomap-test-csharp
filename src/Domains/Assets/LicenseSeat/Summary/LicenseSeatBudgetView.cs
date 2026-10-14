namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using System.Globalization;

/// <summary>How screens and exports show a license seat budget.</summary>
public sealed record LicenseSeatBudgetView(string Code, string Label, string Ratio, string Cost, string JobCode, string AmountText, string HoursText, string Minimum, string LocaleText, string StatusLabel)
{
    public static LicenseSeatBudgetView ToLicenseSeatBudgetView(LicenseSeatBudget item)
    {
        return new LicenseSeatBudgetView(
            item.Code,
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatBudgetView> ToLicenseSeatBudgetViews(List<LicenseSeatBudget> items)
    {
        var views = new List<LicenseSeatBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatBudgetView(item));
        }
        return views;
    }

    public static double SumLicenseSeatBudgetView(LicenseSeatBudgetView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedRatio + parsedCost + parsedAmount;
    }
}
