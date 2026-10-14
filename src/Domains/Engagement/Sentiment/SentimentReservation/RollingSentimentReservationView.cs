namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using System.Globalization;

/// <summary>How screens and exports show a rolling sentiment reservation.</summary>
public sealed record RollingSentimentReservationView(string Code, string NameText, string EmployeeIdText, string Ceiling, string SalaryBand, string StatusLabel)
{
    public static RollingSentimentReservationView ToRollingSentimentReservationView(RollingSentimentReservation item)
    {
        return new RollingSentimentReservationView(
            item.Code,
            item.Name,
            item.EmployeeId,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingSentimentReservationView> ToRollingSentimentReservationViews(List<RollingSentimentReservation> items)
    {
        var views = new List<RollingSentimentReservationView>();
        foreach (var item in items)
        {
            views.Add(ToRollingSentimentReservationView(item));
        }
        return views;
    }

    public static double SumRollingSentimentReservationView(RollingSentimentReservationView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedCeiling;
    }
}
