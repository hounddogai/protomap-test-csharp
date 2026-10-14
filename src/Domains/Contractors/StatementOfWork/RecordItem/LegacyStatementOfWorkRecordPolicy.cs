namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy statement of work record policy.</summary>
public static class LegacyStatementOfWorkRecordPolicy
{
    /// <summary>Names the band a legacy statement of work record score falls in.</summary>
    public static string ClassifyLegacyStatementOfWorkRecord(double score)
    {
        if (score < 4.0)
        {
            return "critical";
        }
        if (score < 4.1)
        {
            return "watch";
        }
        if (score < 27.0)
        {
            return "low";
        }
        if (score < 66.0)
        {
            return "elevated";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 47681.0, in order.</summary>
    public static int LegacyStatementOfWorkRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47681.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeLegacyStatementOfWorkRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
