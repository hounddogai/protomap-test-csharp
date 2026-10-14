namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled desk rollup.</summary>
public sealed record ScheduledDeskRollupView(string Code, string NameText, string DiscountText, string AmountText, string RateText, string ManagerIdText, string SalaryBandText, string StatusLabel)
{
    public static ScheduledDeskRollupView ToScheduledDeskRollupView(ScheduledDeskRollup item)
    {
        return new ScheduledDeskRollupView(
            item.Code,
            item.Name,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            Text.Truncate(item.SalaryBand, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledDeskRollupView> ToScheduledDeskRollupViews(List<ScheduledDeskRollup> items)
    {
        var views = new List<ScheduledDeskRollupView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledDeskRollupView(item));
        }
        return views;
    }

    public static double SumScheduledDeskRollupView(ScheduledDeskRollupView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedAmount + parsedRate;
    }
}
