namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling leave request adjustment.</summary>
public sealed record RollingLeaveRequestAdjustmentView(string Code, string TitleText, string Visible, string CountryText, string Hours, string Department, string Team, string Phone, string Unit, string Category, string Ceiling, string StatusLabel)
{
    public static RollingLeaveRequestAdjustmentView ToRollingLeaveRequestAdjustmentView(RollingLeaveRequestAdjustment item)
    {
        return new RollingLeaveRequestAdjustmentView(
            item.Code,
            item.Title,
            $"{item.Visible}",
            item.Country,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Team,
            Text.Truncate(item.Phone, 64),
            Text.Truncate(item.Unit, 64),
            item.Category,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingLeaveRequestAdjustmentView> ToRollingLeaveRequestAdjustmentViews(List<RollingLeaveRequestAdjustment> items)
    {
        var views = new List<RollingLeaveRequestAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToRollingLeaveRequestAdjustmentView(item));
        }
        return views;
    }

    public static double SumRollingLeaveRequestAdjustmentView(RollingLeaveRequestAdjustmentView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedHours + parsedCeiling;
    }
}
