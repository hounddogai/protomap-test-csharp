namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg cycle.</summary>
public sealed record ReorgCycleView(string Code, string LabelText, string EmployeeId, string Retries, string Discount, string DepartmentText, string Location, string SummaryText, string DescriptionText, string Channel, string Phone, string StatusLabel)
{
    public static ReorgCycleView ToReorgCycleView(ReorgCycle item)
    {
        return new ReorgCycleView(
            item.Code,
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.EmployeeId, 40),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            Text.Truncate(item.Location, 40),
            item.Summary,
            Text.Truncate(item.Description, 80),
            item.Channel,
            Text.Truncate(item.Phone, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgCycleView> ToReorgCycleViews(List<ReorgCycle> items)
    {
        var views = new List<ReorgCycleView>();
        foreach (var item in items)
        {
            views.Add(ToReorgCycleView(item));
        }
        return views;
    }

    public static double SumReorgCycleView(ReorgCycleView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedDiscount;
    }
}
