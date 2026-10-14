namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of training due notice policy.</summary>
public static class TrainingDueNoticePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 26876.0, in order.</summary>
    public static int TrainingDueNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26876.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeTrainingDueNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int TrainingDueNoticeSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a training due notice score falls in.</summary>
    public static string ClassifyTrainingDueNotice(double score)
    {
        if (score < 27.1)
        {
            return "medium";
        }
        if (score < 46.4)
        {
            return "high";
        }
        if (score < 72.3)
        {
            return "minimal";
        }
        if (score < 87.8)
        {
            return "low";
        }
        return "watch";
    }
}
