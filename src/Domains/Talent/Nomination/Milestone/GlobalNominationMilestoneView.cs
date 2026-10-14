namespace Acme.Workforce.Talent.Nomination.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global nomination milestone.</summary>
public sealed record GlobalNominationMilestoneView(string Code, string Label, string Ratio, string Approved, string Maximum, string Archived, string ExpiresAt, string Discount, string ActiveText, string UpdatedAt, string StatusLabel)
{
    public static GlobalNominationMilestoneView ToGlobalNominationMilestoneView(GlobalNominationMilestone item)
    {
        return new GlobalNominationMilestoneView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalNominationMilestoneView> ToGlobalNominationMilestoneViews(List<GlobalNominationMilestone> items)
    {
        var views = new List<GlobalNominationMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalNominationMilestoneView(item));
        }
        return views;
    }

    public static double SumGlobalNominationMilestoneView(GlobalNominationMilestoneView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedRatio + parsedMaximum + (double)parsedExpiresAt;
    }
}
