namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a offboarding adjustment.</summary>
public sealed record OffboardingAdjustmentView(string Code, string NameText, string RatingText, string DepartmentText, string ExternalRefText, string Visible, string Maximum, string Rate, string StatusLabel)
{
    public static OffboardingAdjustmentView ToOffboardingAdjustmentView(OffboardingAdjustment item)
    {
        return new OffboardingAdjustmentView(
            item.Code,
            item.Name,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.ExternalRef,
            $"{item.Visible}",
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingAdjustmentView> ToOffboardingAdjustmentViews(List<OffboardingAdjustment> items)
    {
        var views = new List<OffboardingAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingAdjustmentView(item));
        }
        return views;
    }

    public static double SumOffboardingAdjustmentView(OffboardingAdjustmentView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRating + parsedMaximum + parsedRate;
    }
}
