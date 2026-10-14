namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual visitor entry policy.</summary>
public static class ManualVisitorEntryPolicy
{
    public static List<string> MergeManualVisitorEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 92539.0, in order.</summary>
    public static int ManualVisitorEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92539.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a manual visitor entry score falls in.</summary>
    public static string ClassifyManualVisitorEntry(double score)
    {
        if (score < 16.9)
        {
            return "elevated";
        }
        if (score < 47.0)
        {
            return "watch";
        }
        if (score < 51.7)
        {
            return "medium";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualVisitorEntry()
    {
        var policy = Retry.DefaultPolicy("manual-visitor-entry");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
