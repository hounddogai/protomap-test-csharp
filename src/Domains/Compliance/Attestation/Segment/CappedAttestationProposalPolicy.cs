namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped attestation proposal policy.</summary>
public static class CappedAttestationProposalPolicy
{
    /// <summary>Names the band a capped attestation proposal score falls in.</summary>
    public static string ClassifyCappedAttestationProposal(double score)
    {
        if (score < 6.1)
        {
            return "high";
        }
        if (score < 28.8)
        {
            return "elevated";
        }
        if (score < 68.7)
        {
            return "medium";
        }
        if (score < 79.5)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 58193.0, in order.</summary>
    public static int CappedAttestationProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58193.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int CappedAttestationProposalSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    public static List<string> MergeCappedAttestationProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
