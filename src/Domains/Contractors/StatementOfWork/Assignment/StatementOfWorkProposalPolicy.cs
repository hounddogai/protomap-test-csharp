namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of statement of work proposal policy.</summary>
public static class StatementOfWorkProposalPolicy
{
    /// <summary>Names the band a statement of work proposal score falls in.</summary>
    public static string ClassifyStatementOfWorkProposal(double score)
    {
        if (score < 2.1)
        {
            return "watch";
        }
        if (score < 54.6)
        {
            return "elevated";
        }
        if (score < 79.4)
        {
            return "ok";
        }
        if (score < 96.2)
        {
            return "minimal";
        }
        return "low";
    }

    public static List<string> MergeStatementOfWorkProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
