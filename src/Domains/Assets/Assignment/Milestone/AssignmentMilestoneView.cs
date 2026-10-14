namespace Acme.Workforce.Assets.Assignment.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment milestone.</summary>
public sealed record AssignmentMilestoneView(string Code, string Label, string Discount, string LocationText, string Channel, string Grade, string MinimumText, string Country, string SalaryBandText, string Region, string Sequence, string StatusLabel)
{
    public static AssignmentMilestoneView ToAssignmentMilestoneView(AssignmentMilestone item)
    {
        return new AssignmentMilestoneView(
            item.Code,
            item.Label,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Channel,
            item.Grade,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 120),
            Text.Truncate(item.SalaryBand, 120),
            Text.Truncate(item.Region, 80),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentMilestoneView> ToAssignmentMilestoneViews(List<AssignmentMilestone> items)
    {
        var views = new List<AssignmentMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentMilestoneView(item));
        }
        return views;
    }

    public static double SumAssignmentMilestoneView(AssignmentMilestoneView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedMinimum + (double)parsedSequence;
    }
}
