namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary recognition variance.</summary>
public sealed record PrimaryRecognitionVarianceView(string CodeText, string TitleText, string Weight, string LocaleText, string AmountText, string DurationMinutes, string Retries, string Ceiling, string Approved, string StatusLabel)
{
    public static PrimaryRecognitionVarianceView ToPrimaryRecognitionVarianceView(PrimaryRecognitionVariance item)
    {
        return new PrimaryRecognitionVarianceView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryRecognitionVarianceView> ToPrimaryRecognitionVarianceViews(List<PrimaryRecognitionVariance> items)
    {
        var views = new List<PrimaryRecognitionVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryRecognitionVarianceView(item));
        }
        return views;
    }

    public static double SumPrimaryRecognitionVarianceView(PrimaryRecognitionVarianceView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedWeight + parsedAmount + (double)parsedDurationMinutes;
    }
}
