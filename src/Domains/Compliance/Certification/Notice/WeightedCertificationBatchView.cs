namespace Acme.Workforce.Compliance.Certification.Notice;

using System.Globalization;

/// <summary>How screens and exports show a weighted certification batch.</summary>
public sealed record WeightedCertificationBatchView(string CodeText, string Title, string Limit, string DurationMinutes, string EmployeeIdText, string Phone, string Ratio, string BudgetText, string Country, string StatusLabel)
{
    public static WeightedCertificationBatchView ToWeightedCertificationBatchView(WeightedCertificationBatch item)
    {
        return new WeightedCertificationBatchView(
            item.Code,
            item.Title,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Phone,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedCertificationBatchView> ToWeightedCertificationBatchViews(List<WeightedCertificationBatch> items)
    {
        var views = new List<WeightedCertificationBatchView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedCertificationBatchView(item));
        }
        return views;
    }

    public static double SumWeightedCertificationBatchView(WeightedCertificationBatchView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedDurationMinutes + parsedRatio;
    }
}
