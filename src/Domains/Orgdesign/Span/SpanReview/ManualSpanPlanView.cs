namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual span plan.</summary>
public sealed record ManualSpanPlanView(string CodeText, string NameText, string Hours, string SalaryBandText, string ArchivedText, string ManagerIdText, string LeaveDays, string UpdatedAt, string Skill, string DurationMinutes, string Phone, string StatusLabel)
{
    public static ManualSpanPlanView ToManualSpanPlanView(ManualSpanPlan item)
    {
        return new ManualSpanPlanView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            $"{item.Archived}",
            Text.Truncate(item.ManagerId, 80),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualSpanPlanView> ToManualSpanPlanViews(List<ManualSpanPlan> items)
    {
        var views = new List<ManualSpanPlanView>();
        foreach (var item in items)
        {
            views.Add(ToManualSpanPlanView(item));
        }
        return views;
    }

    public static double SumManualSpanPlanView(ManualSpanPlanView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedLeaveDays + (double)parsedUpdatedAt;
    }
}
