namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary engagement profile.</summary>
public sealed record PrimaryEngagementProfileView(string Code, string Title, string MaximumText, string EndsAtText, string Skill, string VisibleText, string Source, string ChecksumText, string Channel, string Budget, string StatusLabel)
{
    public static PrimaryEngagementProfileView ToPrimaryEngagementProfileView(PrimaryEngagementProfile item)
    {
        return new PrimaryEngagementProfileView(
            item.Code,
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Visible}",
            Text.Truncate(item.Source, 64),
            item.Checksum,
            item.Channel,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryEngagementProfileView> ToPrimaryEngagementProfileViews(List<PrimaryEngagementProfile> items)
    {
        var views = new List<PrimaryEngagementProfileView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryEngagementProfileView(item));
        }
        return views;
    }

    public static double SumPrimaryEngagementProfileView(PrimaryEngagementProfileView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedEndsAt + parsedBudget;
    }
}
