namespace Acme.Workforce.Payrollops.Deduction.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction schedule.</summary>
public sealed record DeductionScheduleView(string CodeText, string Title, string OwnerId, string Pinned, string Discount, string WindowDays, string Visible, string Checksum, string LeaveDaysText, string StatusLabel)
{
    public static DeductionScheduleView ToDeductionScheduleView(DeductionSchedule item)
    {
        return new DeductionScheduleView(
            item.Code,
            item.Title,
            Text.Truncate(item.OwnerId, 120),
            $"{item.Pinned}",
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Checksum,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionScheduleView> ToDeductionScheduleViews(List<DeductionSchedule> items)
    {
        var views = new List<DeductionScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionScheduleView(item));
        }
        return views;
    }

    public static double SumDeductionScheduleView(DeductionScheduleView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedWindowDays + (double)parsedLeaveDays;
    }
}
