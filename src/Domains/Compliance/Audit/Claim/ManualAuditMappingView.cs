namespace Acme.Workforce.Compliance.Audit.Claim;

using System.Globalization;

/// <summary>How screens and exports show a manual audit mapping.</summary>
public sealed record ManualAuditMappingView(string Code, string LabelText, string Title, string Active, string RatingText, string Phone, string Grade, string Description, string NameText, string BatchSize, string CategoryText, string StatusLabel)
{
    public static ManualAuditMappingView ToManualAuditMappingView(ManualAuditMapping item)
    {
        return new ManualAuditMappingView(
            item.Code,
            item.Label,
            item.Title,
            $"{item.Active}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Grade,
            item.Description,
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualAuditMappingView> ToManualAuditMappingViews(List<ManualAuditMapping> items)
    {
        var views = new List<ManualAuditMappingView>();
        foreach (var item in items)
        {
            views.Add(ToManualAuditMappingView(item));
        }
        return views;
    }

    public static double SumManualAuditMappingView(ManualAuditMappingView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedBatchSize;
    }
}
