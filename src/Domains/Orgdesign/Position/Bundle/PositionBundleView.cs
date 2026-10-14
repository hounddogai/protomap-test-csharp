namespace Acme.Workforce.Orgdesign.Position.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position bundle.</summary>
public sealed record PositionBundleView(string Code, string Name, string DepartmentText, string Unit, string Summary, string EmployeeId, string Amount, string StatusLabel)
{
    public static PositionBundleView ToPositionBundleView(PositionBundle item)
    {
        return new PositionBundleView(
            item.Code,
            Text.Truncate(item.Name, 64),
            Text.Truncate(item.Department, 120),
            item.Unit,
            item.Summary,
            item.EmployeeId,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionBundleView> ToPositionBundleViews(List<PositionBundle> items)
    {
        var views = new List<PositionBundleView>();
        foreach (var item in items)
        {
            views.Add(ToPositionBundleView(item));
        }
        return views;
    }

    public static double SumPositionBundleView(PositionBundleView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
