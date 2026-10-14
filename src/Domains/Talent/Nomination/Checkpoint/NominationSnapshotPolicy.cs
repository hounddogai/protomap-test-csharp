namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of nomination snapshot policy.</summary>
public static class NominationSnapshotPolicy
{
    public static List<string> MergeNominationSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a nomination snapshot score falls in.</summary>
    public static string ClassifyNominationSnapshot(double score)
    {
        if (score < 25.6)
        {
            return "watch";
        }
        if (score < 28.1)
        {
            return "critical";
        }
        if (score < 64.6)
        {
            return "high";
        }
        if (score < 74.5)
        {
            return "medium";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 62310.0, in order.</summary>
    public static int NominationSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 62310.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
