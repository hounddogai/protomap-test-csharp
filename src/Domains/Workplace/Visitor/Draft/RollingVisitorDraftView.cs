namespace Acme.Workforce.Workplace.Visitor.Draft;

using System.Globalization;

/// <summary>How screens and exports show a rolling visitor draft.</summary>
public sealed record RollingVisitorDraftView(string CodeText, string TitleText, string PriorityText, string UpdatedAt, string Pinned, string Team, string Capacity, string ReviewerText, string CostCenter, string TimezoneText, string StatusLabel)
{
    public static RollingVisitorDraftView ToRollingVisitorDraftView(RollingVisitorDraft item)
    {
        return new RollingVisitorDraftView(
            item.Code,
            item.Title,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Team,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.CostCenter,
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingVisitorDraftView> ToRollingVisitorDraftViews(List<RollingVisitorDraft> items)
    {
        var views = new List<RollingVisitorDraftView>();
        foreach (var item in items)
        {
            views.Add(ToRollingVisitorDraftView(item));
        }
        return views;
    }

    public static double SumRollingVisitorDraftView(RollingVisitorDraftView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedUpdatedAt + (double)parsedCapacity;
    }
}
