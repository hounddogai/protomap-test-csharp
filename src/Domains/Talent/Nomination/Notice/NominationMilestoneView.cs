namespace Acme.Workforce.Talent.Nomination.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination milestone.</summary>
public sealed record NominationMilestoneView(string CodeText, string Title, string RevisionText, string Capacity, string JobCode, string GraceDays, string Phone, string Minimum, string ActiveText, string StatusLabel)
{
    public static NominationMilestoneView ToNominationMilestoneView(NominationMilestone item)
    {
        return new NominationMilestoneView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 80),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationMilestoneView> ToNominationMilestoneViews(List<NominationMilestone> items)
    {
        var views = new List<NominationMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToNominationMilestoneView(item));
        }
        return views;
    }

    public static double SumNominationMilestoneView(NominationMilestoneView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedCapacity + (double)parsedGraceDays;
    }
}
