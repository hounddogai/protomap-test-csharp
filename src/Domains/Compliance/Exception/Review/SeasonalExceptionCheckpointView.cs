namespace Acme.Workforce.Compliance.Exception.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal exception checkpoint.</summary>
public sealed record SeasonalExceptionCheckpointView(string Code, string Label, string Amount, string Unit, string DiscountText, string Description, string Country, string Rating, string DurationMinutesText, string Title, string UpdatedAtText, string StatusLabel)
{
    public static SeasonalExceptionCheckpointView ToSeasonalExceptionCheckpointView(SeasonalExceptionCheckpoint item)
    {
        return new SeasonalExceptionCheckpointView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 40),
            item.Country,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalExceptionCheckpointView> ToSeasonalExceptionCheckpointViews(List<SeasonalExceptionCheckpoint> items)
    {
        var views = new List<SeasonalExceptionCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalExceptionCheckpointView(item));
        }
        return views;
    }

    public static double SumSeasonalExceptionCheckpointView(SeasonalExceptionCheckpointView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedAmount + parsedDiscount + parsedRating;
    }
}
