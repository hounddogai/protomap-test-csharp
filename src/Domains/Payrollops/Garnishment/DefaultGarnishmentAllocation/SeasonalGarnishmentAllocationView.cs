namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal garnishment allocation.</summary>
public sealed record SeasonalGarnishmentAllocationView(string Code, string Title, string Unit, string Grade, string Department, string CurrencyText, string Threshold, string Active, string CostCenter, string Attempts, string GraceDaysText, string StatusLabel)
{
    public static SeasonalGarnishmentAllocationView ToSeasonalGarnishmentAllocationView(SeasonalGarnishmentAllocation item)
    {
        return new SeasonalGarnishmentAllocationView(
            item.Code,
            item.Title,
            item.Unit,
            item.Grade,
            item.Department,
            Text.Truncate(item.Currency, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.CostCenter,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalGarnishmentAllocationView> ToSeasonalGarnishmentAllocationViews(List<SeasonalGarnishmentAllocation> items)
    {
        var views = new List<SeasonalGarnishmentAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalGarnishmentAllocationView(item));
        }
        return views;
    }

    public static double SumSeasonalGarnishmentAllocationView(SeasonalGarnishmentAllocationView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedAttempts + (double)parsedGraceDays;
    }
}
