namespace Acme.Workforce.Compliance.Audit.Manifest;

using System.Globalization;

/// <summary>How screens and exports show a audit estimate.</summary>
public sealed record AuditEstimateView(string Code, string NameText, string ExpiresAt, string RatingText, string Checksum, string Visible, string Cost, string EmployeeIdText, string ScoreText, string Grade, string Weight, string StatusLabel)
{
    public static AuditEstimateView ToAuditEstimateView(AuditEstimate item)
    {
        return new AuditEstimateView(
            item.Code,
            item.Name,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Visible}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditEstimateView> ToAuditEstimateViews(List<AuditEstimate> items)
    {
        var views = new List<AuditEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToAuditEstimateView(item));
        }
        return views;
    }

    public static double SumAuditEstimateView(AuditEstimateView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedRating + parsedCost;
    }
}
