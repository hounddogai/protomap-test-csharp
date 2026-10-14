namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of leave request proposal policy.</summary>
public static class LeaveRequestProposalPolicy
{
    /// <summary>Puts a leave request proposal code in its canonical LRP- form.</summary>
    public static string NormalizeLeaveRequestProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a leave request proposal score falls in.</summary>
    public static string ClassifyLeaveRequestProposal(double score)
    {
        if (score < 25.6)
        {
            return "low";
        }
        if (score < 70.1)
        {
            return "elevated";
        }
        if (score < 73.9)
        {
            return "ok";
        }
        if (score < 95.9)
        {
            return "watch";
        }
        return "critical";
    }

    public static int LeaveRequestProposalSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static string LeaveRequestProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
