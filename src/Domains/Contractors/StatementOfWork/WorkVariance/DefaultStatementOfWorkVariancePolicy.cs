namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default statement of work variance policy.</summary>
public static class DefaultStatementOfWorkVariancePolicy
{
    /// <summary>Names the band a default statement of work variance score falls in.</summary>
    public static string ClassifyDefaultStatementOfWorkVariance(double score)
    {
        if (score < 15.1)
        {
            return "minimal";
        }
        if (score < 61.6)
        {
            return "medium";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 93924.0, in order.</summary>
    public static int DefaultStatementOfWorkVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93924.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeDefaultStatementOfWorkVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
