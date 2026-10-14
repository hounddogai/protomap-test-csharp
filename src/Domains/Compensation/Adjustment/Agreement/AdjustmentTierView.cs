namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment tier.</summary>
public sealed record AdjustmentTierView(string Code, string Title, string Active, string JobCode, string UpdatedAt, string Locale, string EmployeeId, string WindowDays, string GradeText, string TenureMonthsText, string UnitText, string StatusLabel)
{
    public static AdjustmentTierView ToAdjustmentTierView(AdjustmentTier item)
    {
        return new AdjustmentTierView(
            item.Code,
            item.Title,
            $"{item.Active}",
            Text.Truncate(item.JobCode, 64),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 120),
            Text.Truncate(item.EmployeeId, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentTierView> ToAdjustmentTierViews(List<AdjustmentTier> items)
    {
        var views = new List<AdjustmentTierView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentTierView(item));
        }
        return views;
    }

    public static double SumAdjustmentTierView(AdjustmentTierView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedWindowDays + (double)parsedTenureMonths;
    }
}
