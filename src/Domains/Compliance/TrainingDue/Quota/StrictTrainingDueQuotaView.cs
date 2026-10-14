namespace Acme.Workforce.Compliance.TrainingDue.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict training due quota.</summary>
public sealed record StrictTrainingDueQuotaView(string Code, string Label, string Summary, string RevisionText, string Team, string PinnedText, string StatusLabel)
{
    public static StrictTrainingDueQuotaView ToStrictTrainingDueQuotaView(StrictTrainingDueQuota item)
    {
        return new StrictTrainingDueQuotaView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Summary,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictTrainingDueQuotaView> ToStrictTrainingDueQuotaViews(List<StrictTrainingDueQuota> items)
    {
        var views = new List<StrictTrainingDueQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToStrictTrainingDueQuotaView(item));
        }
        return views;
    }

    public static double SumStrictTrainingDueQuotaView(StrictTrainingDueQuotaView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
