namespace Acme.Workforce.Absence.Holiday.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday tier.</summary>
public sealed record HolidayTierView(string Code, string TitleText, string EmployeeIdText, string Pinned, string GraceDays, string RatingText, string Visible, string StatusLabel)
{
    public static HolidayTierView ToHolidayTierView(HolidayTier item)
    {
        return new HolidayTierView(
            item.Code,
            item.Title,
            Text.Truncate(item.EmployeeId, 40),
            $"{item.Pinned}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayTierView> ToHolidayTierViews(List<HolidayTier> items)
    {
        var views = new List<HolidayTierView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayTierView(item));
        }
        return views;
    }

    public static double SumHolidayTierView(HolidayTierView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedRating;
    }
}
