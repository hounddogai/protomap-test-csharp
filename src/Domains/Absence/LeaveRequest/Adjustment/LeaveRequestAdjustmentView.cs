namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request adjustment.</summary>
public sealed record LeaveRequestAdjustmentView(string Code, string TitleText, string Amount, string Grade, string ManagerId, string CountryText, string CurrencyText, string Team, string StatusLabel)
{
    public static LeaveRequestAdjustmentView ToLeaveRequestAdjustmentView(LeaveRequestAdjustment item)
    {
        return new LeaveRequestAdjustmentView(
            item.Code,
            item.Title,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.ManagerId,
            item.Country,
            Text.Truncate(item.Currency, 120),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestAdjustmentView> ToLeaveRequestAdjustmentViews(List<LeaveRequestAdjustment> items)
    {
        var views = new List<LeaveRequestAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestAdjustmentView(item));
        }
        return views;
    }

    public static double SumLeaveRequestAdjustmentView(LeaveRequestAdjustmentView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
