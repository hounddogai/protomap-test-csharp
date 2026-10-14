namespace Acme.Workforce.Payrollops.Deduction.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global deduction reservation.</summary>
public sealed record GlobalDeductionReservationView(string CodeText, string Title, string Segment, string Score, string Category, string Amount, string WeightText, string RatingText, string StatusLabel)
{
    public static GlobalDeductionReservationView ToGlobalDeductionReservationView(GlobalDeductionReservation item)
    {
        return new GlobalDeductionReservationView(
            Text.Truncate(item.Code, 80),
            item.Title,
            Text.Truncate(item.Segment, 80),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalDeductionReservationView> ToGlobalDeductionReservationViews(List<GlobalDeductionReservation> items)
    {
        var views = new List<GlobalDeductionReservationView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalDeductionReservationView(item));
        }
        return views;
    }

    public static double SumGlobalDeductionReservationView(GlobalDeductionReservationView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return parsedScore + parsedAmount + parsedWeight;
    }
}
