namespace Acme.Workforce.Engagement.TownHall.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive town hall score.</summary>
public sealed record AdaptiveTownHallScoreView(string CodeText, string Name, string Phone, string FteText, string DiscountText, string DurationMinutes, string SalaryBand, string StatusLabel)
{
    public static AdaptiveTownHallScoreView ToAdaptiveTownHallScoreView(AdaptiveTownHallScore item)
    {
        return new AdaptiveTownHallScoreView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Phone,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveTownHallScoreView> ToAdaptiveTownHallScoreViews(List<AdaptiveTownHallScore> items)
    {
        var views = new List<AdaptiveTownHallScoreView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveTownHallScoreView(item));
        }
        return views;
    }

    public static double SumAdaptiveTownHallScoreView(AdaptiveTownHallScoreView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedFte + parsedDiscount + (double)parsedDurationMinutes;
    }
}
