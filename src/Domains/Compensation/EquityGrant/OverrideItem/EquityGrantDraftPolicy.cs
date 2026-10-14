namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of equity grant draft policy.</summary>
public static class EquityGrantDraftPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeEquityGrantDraft()
    {
        var policy = Retry.DefaultPolicy("equity-grant-draft");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a equity grant draft code in its canonical EGD- form.</summary>
    public static string NormalizeEquityGrantDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EGD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EGD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
