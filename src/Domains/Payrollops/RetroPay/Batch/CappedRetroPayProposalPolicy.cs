namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped retro pay proposal policy.</summary>
public static class CappedRetroPayProposalPolicy
{
    public static List<string> MergeCappedRetroPayProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedRetroPayProposal()
    {
        var policy = Retry.DefaultPolicy("capped-retro-pay-proposal");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a capped retro pay proposal code in its canonical CRPP- form.</summary>
    public static string NormalizeCappedRetroPayProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CRPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CRPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a capped retro pay proposal score falls in.</summary>
    public static string ClassifyCappedRetroPayProposal(double score)
    {
        if (score < 42.6)
        {
            return "watch";
        }
        if (score < 42.8)
        {
            return "high";
        }
        if (score < 51.5)
        {
            return "low";
        }
        if (score < 59.4)
        {
            return "minimal";
        }
        return "ok";
    }
}
