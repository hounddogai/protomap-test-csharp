namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary bonus pool group.</summary>
public sealed record PrimaryBonusPoolGroupView(string Code, string Name, string Sequence, string EndsAtText, string Version, string EmployeeId, string BatchSizeText, string Skill, string WindowDays, string Segment, string StatusLabel)
{
    public static PrimaryBonusPoolGroupView ToPrimaryBonusPoolGroupView(PrimaryBonusPoolGroup item)
    {
        return new PrimaryBonusPoolGroupView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 80),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryBonusPoolGroupView> ToPrimaryBonusPoolGroupViews(List<PrimaryBonusPoolGroup> items)
    {
        var views = new List<PrimaryBonusPoolGroupView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryBonusPoolGroupView(item));
        }
        return views;
    }

    public static double SumPrimaryBonusPoolGroupView(PrimaryBonusPoolGroupView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedEndsAt + (double)parsedVersion;
    }
}
