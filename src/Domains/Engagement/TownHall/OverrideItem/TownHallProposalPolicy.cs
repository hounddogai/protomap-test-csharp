namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of town hall proposal policy.</summary>
public static class TownHallProposalPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTownHallProposal()
    {
        var policy = Retry.DefaultPolicy("town-hall-proposal");
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

    public static int TownHallProposalSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a town hall proposal code in its canonical THP- form.</summary>
    public static string NormalizeTownHallProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("THP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"THP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
