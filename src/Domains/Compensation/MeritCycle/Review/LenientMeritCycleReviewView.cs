namespace Acme.Workforce.Compensation.MeritCycle.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient merit cycle review.</summary>
public sealed record LenientMeritCycleReviewView(string CodeText, string Title, string Quantity, string Version, string Category, string Unit, string Locale, string Maximum, string Attempts, string StatusLabel)
{
    public static LenientMeritCycleReviewView ToLenientMeritCycleReviewView(LenientMeritCycleReview item)
    {
        return new LenientMeritCycleReviewView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 80),
            item.Unit,
            item.Locale,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientMeritCycleReviewView> ToLenientMeritCycleReviewViews(List<LenientMeritCycleReview> items)
    {
        var views = new List<LenientMeritCycleReviewView>();
        foreach (var item in items)
        {
            views.Add(ToLenientMeritCycleReviewView(item));
        }
        return views;
    }

    public static double SumLenientMeritCycleReviewView(LenientMeritCycleReviewView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedVersion + parsedMaximum;
    }
}
