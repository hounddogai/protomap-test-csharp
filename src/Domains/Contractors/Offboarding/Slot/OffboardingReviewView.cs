namespace Acme.Workforce.Contractors.Offboarding.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding review.</summary>
public sealed record OffboardingReviewView(string Code, string Label, string Amount, string AttemptsText, string Country, string Fte, string Segment, string Weight, string CategoryText, string StatusLabel)
{
    public static OffboardingReviewView ToOffboardingReviewView(OffboardingReview item)
    {
        return new OffboardingReviewView(
            item.Code,
            item.Label,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 40),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingReviewView> ToOffboardingReviewViews(List<OffboardingReview> items)
    {
        var views = new List<OffboardingReviewView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingReviewView(item));
        }
        return views;
    }

    public static double SumOffboardingReviewView(OffboardingReviewView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedAttempts + parsedFte;
    }
}
