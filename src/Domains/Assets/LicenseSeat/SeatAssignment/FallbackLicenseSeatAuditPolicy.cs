namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback license seat audit policy.</summary>
public static class FallbackLicenseSeatAuditPolicy
{
    public static List<string> MergeFallbackLicenseSeatAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a fallback license seat audit code in its canonical FLSA- form.</summary>
    public static string NormalizeFallbackLicenseSeatAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FLSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FLSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackLicenseSeatAudit()
    {
        var policy = Retry.DefaultPolicy("fallback-license-seat-audit");
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
}
