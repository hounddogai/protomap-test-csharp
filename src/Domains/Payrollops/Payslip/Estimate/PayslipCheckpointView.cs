namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip checkpoint.</summary>
public sealed record PayslipCheckpointView(string Code, string Name, string Visible, string CreatedAtText, string SkillText, string Rate, string Weight, string Hours, string Notes, string StartsAt, string StatusLabel)
{
    public static PayslipCheckpointView ToPayslipCheckpointView(PayslipCheckpoint item)
    {
        return new PayslipCheckpointView(
            item.Code,
            item.Name,
            $"{item.Visible}",
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 64),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipCheckpointView> ToPayslipCheckpointViews(List<PayslipCheckpoint> items)
    {
        var views = new List<PayslipCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipCheckpointView(item));
        }
        return views;
    }

    public static double SumPayslipCheckpointView(PayslipCheckpointView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedRate + parsedWeight;
    }
}
