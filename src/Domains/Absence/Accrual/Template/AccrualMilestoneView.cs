namespace Acme.Workforce.Absence.Accrual.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual milestone.</summary>
public sealed record AccrualMilestoneView(string Code, string Name, string Pinned, string UpdatedAt, string EndsAtText, string Country, string StatusLabel)
{
    public static AccrualMilestoneView ToAccrualMilestoneView(AccrualMilestone item)
    {
        return new AccrualMilestoneView(
            item.Code,
            item.Name,
            $"{item.Pinned}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualMilestoneView> ToAccrualMilestoneViews(List<AccrualMilestone> items)
    {
        var views = new List<AccrualMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualMilestoneView(item));
        }
        return views;
    }

    public static double SumAccrualMilestoneView(AccrualMilestoneView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedEndsAt;
    }
}
