namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using System.Globalization;

/// <summary>How screens and exports show a primary offboarding review.</summary>
public sealed record PrimaryOffboardingReviewView(string Code, string Label, string EndsAtText, string Segment, string Budget, string Capacity, string DurationMinutes, string FloorValue, string Minimum, string Ceiling, string StatusLabel)
{
    public static PrimaryOffboardingReviewView ToPrimaryOffboardingReviewView(PrimaryOffboardingReview item)
    {
        return new PrimaryOffboardingReviewView(
            item.Code,
            item.Label,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryOffboardingReviewView> ToPrimaryOffboardingReviewViews(List<PrimaryOffboardingReview> items)
    {
        var views = new List<PrimaryOffboardingReviewView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryOffboardingReviewView(item));
        }
        return views;
    }

    public static double SumPrimaryOffboardingReviewView(PrimaryOffboardingReviewView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedBudget + (double)parsedCapacity;
    }
}
