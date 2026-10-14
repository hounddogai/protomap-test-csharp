namespace Acme.Workforce.Assets.Warranty.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty assignment.</summary>
public sealed record WarrantyAssignmentView(string CodeText, string Name, string LeaveDays, string StartsAt, string RatioText, string Department, string StatusLabel)
{
    public static WarrantyAssignmentView ToWarrantyAssignmentView(WarrantyAssignment item)
    {
        return new WarrantyAssignmentView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyAssignmentView> ToWarrantyAssignmentViews(List<WarrantyAssignment> items)
    {
        var views = new List<WarrantyAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyAssignmentView(item));
        }
        return views;
    }

    public static double SumWarrantyAssignmentView(WarrantyAssignmentView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedStartsAt + parsedRatio;
    }
}
