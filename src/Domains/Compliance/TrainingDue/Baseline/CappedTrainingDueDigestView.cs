namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped training due digest.</summary>
public sealed record CappedTrainingDueDigestView(string Code, string Name, string StartsAt, string Approved, string Pinned, string ReviewerText, string Team, string CostCenterText, string StatusLabel)
{
    public static CappedTrainingDueDigestView ToCappedTrainingDueDigestView(CappedTrainingDueDigest item)
    {
        return new CappedTrainingDueDigestView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Pinned}",
            Text.Truncate(item.Reviewer, 80),
            Text.Truncate(item.Team, 40),
            Text.Truncate(item.CostCenter, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedTrainingDueDigestView> ToCappedTrainingDueDigestViews(List<CappedTrainingDueDigest> items)
    {
        var views = new List<CappedTrainingDueDigestView>();
        foreach (var item in items)
        {
            views.Add(ToCappedTrainingDueDigestView(item));
        }
        return views;
    }

    public static double SumCappedTrainingDueDigestView(CappedTrainingDueDigestView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
