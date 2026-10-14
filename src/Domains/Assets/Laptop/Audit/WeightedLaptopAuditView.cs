namespace Acme.Workforce.Assets.Laptop.Audit;

using System.Globalization;

/// <summary>How screens and exports show a weighted laptop audit.</summary>
public sealed record WeightedLaptopAuditView(string Code, string LabelText, string Notes, string DurationMinutesText, string Reviewer, string ScoreText, string Region, string ExpiresAt, string Hours, string Category, string StatusLabel)
{
    public static WeightedLaptopAuditView ToWeightedLaptopAuditView(WeightedLaptopAudit item)
    {
        return new WeightedLaptopAuditView(
            item.Code,
            item.Label,
            item.Notes,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedLaptopAuditView> ToWeightedLaptopAuditViews(List<WeightedLaptopAudit> items)
    {
        var views = new List<WeightedLaptopAuditView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedLaptopAuditView(item));
        }
        return views;
    }

    public static double SumWeightedLaptopAuditView(WeightedLaptopAuditView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedScore + (double)parsedExpiresAt;
    }
}
