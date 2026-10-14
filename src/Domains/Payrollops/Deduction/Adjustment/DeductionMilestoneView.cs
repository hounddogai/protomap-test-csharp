namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction milestone.</summary>
public sealed record DeductionMilestoneView(string Code, string Name, string Maximum, string JobCode, string Weight, string ExternalRefText, string Badge, string UpdatedAt, string Minimum, string StatusLabel)
{
    public static DeductionMilestoneView ToDeductionMilestoneView(DeductionMilestone item)
    {
        return new DeductionMilestoneView(
            item.Code,
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 120),
            item.Badge,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionMilestoneView> ToDeductionMilestoneViews(List<DeductionMilestone> items)
    {
        var views = new List<DeductionMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionMilestoneView(item));
        }
        return views;
    }

    public static double SumDeductionMilestoneView(DeductionMilestoneView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedWeight + (double)parsedUpdatedAt;
    }
}
