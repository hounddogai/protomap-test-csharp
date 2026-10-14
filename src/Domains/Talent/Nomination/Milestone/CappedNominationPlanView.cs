namespace Acme.Workforce.Talent.Nomination.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped nomination plan.</summary>
public sealed record CappedNominationPlanView(string CodeText, string Label, string Notes, string SummaryText, string ExpiresAtText, string Revision, string Team, string Timezone, string JobCode, string StatusLabel)
{
    public static CappedNominationPlanView ToCappedNominationPlanView(CappedNominationPlan item)
    {
        return new CappedNominationPlanView(
            item.Code,
            item.Label,
            item.Notes,
            item.Summary,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.Timezone,
            Text.Truncate(item.JobCode, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedNominationPlanView> ToCappedNominationPlanViews(List<CappedNominationPlan> items)
    {
        var views = new List<CappedNominationPlanView>();
        foreach (var item in items)
        {
            views.Add(ToCappedNominationPlanView(item));
        }
        return views;
    }

    public static double SumCappedNominationPlanView(CappedNominationPlanView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedRevision;
    }
}
