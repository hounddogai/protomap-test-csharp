namespace Acme.Workforce.Talent.Mobility.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility metric.</summary>
public sealed record MobilityMetricView(string Code, string NameText, string DepartmentText, string Grade, string Approved, string Quantity, string CapacityText, string Minimum, string Unit, string ActiveText, string StatusLabel)
{
    public static MobilityMetricView ToMobilityMetricView(MobilityMetric item)
    {
        return new MobilityMetricView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Department,
            item.Grade,
            $"{item.Approved}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityMetricView> ToMobilityMetricViews(List<MobilityMetric> items)
    {
        var views = new List<MobilityMetricView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityMetricView(item));
        }
        return views;
    }

    public static double SumMobilityMetricView(MobilityMetricView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedCapacity + parsedMinimum;
    }
}
