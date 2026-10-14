namespace Acme.Workforce.Absence.Accrual.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary accrual schedule.</summary>
public sealed record PrimaryAccrualScheduleView(string Code, string Label, string TitleText, string NameText, string Channel, string SourceText, string Budget, string DiscountText, string SalaryBandText, string BadgeText, string RatingText, string StatusLabel)
{
    public static PrimaryAccrualScheduleView ToPrimaryAccrualScheduleView(PrimaryAccrualSchedule item)
    {
        return new PrimaryAccrualScheduleView(
            item.Code,
            item.Label,
            item.Title,
            Text.Truncate(item.Name, 80),
            Text.Truncate(item.Channel, 120),
            item.Source,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            Text.Truncate(item.Badge, 120),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryAccrualScheduleView> ToPrimaryAccrualScheduleViews(List<PrimaryAccrualSchedule> items)
    {
        var views = new List<PrimaryAccrualScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryAccrualScheduleView(item));
        }
        return views;
    }

    public static double SumPrimaryAccrualScheduleView(PrimaryAccrualScheduleView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return parsedBudget + parsedDiscount + parsedRating;
    }
}
