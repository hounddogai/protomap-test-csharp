namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using System.Globalization;

/// <summary>How screens and exports show a equity grant batch.</summary>
public sealed record EquityGrantBatchView(string Code, string TitleText, string EmployeeId, string Weight, string VisibleText, string Skill, string Hours, string Margin, string Badge, string Minimum, string BatchSize, string StatusLabel)
{
    public static EquityGrantBatchView ToEquityGrantBatchView(EquityGrantBatch item)
    {
        return new EquityGrantBatchView(
            item.Code,
            item.Title,
            item.EmployeeId,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Skill,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantBatchView> ToEquityGrantBatchViews(List<EquityGrantBatch> items)
    {
        var views = new List<EquityGrantBatchView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantBatchView(item));
        }
        return views;
    }

    public static double SumEquityGrantBatchView(EquityGrantBatchView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedWeight + parsedHours + parsedMargin;
    }
}
