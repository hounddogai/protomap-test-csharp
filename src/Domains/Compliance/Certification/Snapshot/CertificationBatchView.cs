namespace Acme.Workforce.Compliance.Certification.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a certification batch.</summary>
public sealed record CertificationBatchView(string Code, string Title, string Active, string Label, string ContactEmailText, string Minimum, string Quantity, string FloorValueText, string DepartmentText, string StatusLabel)
{
    public static CertificationBatchView ToCertificationBatchView(CertificationBatch item)
    {
        return new CertificationBatchView(
            item.Code,
            item.Title,
            $"{item.Active}",
            item.Label,
            item.ContactEmail,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationBatchView> ToCertificationBatchViews(List<CertificationBatch> items)
    {
        var views = new List<CertificationBatchView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationBatchView(item));
        }
        return views;
    }

    public static double SumCertificationBatchView(CertificationBatchView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedQuantity + parsedFloorValue;
    }
}
