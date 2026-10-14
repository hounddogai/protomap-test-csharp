namespace Acme.Workforce.Assets.Laptop.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop cycle.</summary>
public sealed record LaptopCycleView(string Code, string Name, string Threshold, string DepartmentText, string LeaveDaysText, string SummaryText, string Margin, string CapacityText, string Hours, string Limit, string ExpiresAt, string StatusLabel)
{
    public static LaptopCycleView ToLaptopCycleView(LaptopCycle item)
    {
        return new LaptopCycleView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 120),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopCycleView> ToLaptopCycleViews(List<LaptopCycle> items)
    {
        var views = new List<LaptopCycleView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopCycleView(item));
        }
        return views;
    }

    public static double SumLaptopCycleView(LaptopCycleView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedLeaveDays + parsedMargin;
    }
}
