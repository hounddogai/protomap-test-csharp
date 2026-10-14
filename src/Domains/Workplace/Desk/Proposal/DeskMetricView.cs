namespace Acme.Workforce.Workplace.Desk.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk metric.</summary>
public sealed record DeskMetricView(string CodeText, string NameText, string StartsAt, string Maximum, string EmployeeId, string LocationText, string CostCenterText, string Visible, string RatioText, string Quantity, string Ceiling, string StatusLabel)
{
    public static DeskMetricView ToDeskMetricView(DeskMetric item)
    {
        return new DeskMetricView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Location,
            item.CostCenter,
            $"{item.Visible}",
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskMetricView> ToDeskMetricViews(List<DeskMetric> items)
    {
        var views = new List<DeskMetricView>();
        foreach (var item in items)
        {
            views.Add(ToDeskMetricView(item));
        }
        return views;
    }

    public static double SumDeskMetricView(DeskMetricView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedMaximum + parsedRatio;
    }
}
