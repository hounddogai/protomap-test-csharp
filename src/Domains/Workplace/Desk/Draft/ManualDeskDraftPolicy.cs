namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual desk draft policy.</summary>
public static class ManualDeskDraftPolicy
{
    /// <summary>Puts a manual desk draft code in its canonical MDD- form.</summary>
    public static string NormalizeManualDeskDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MDD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MDD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeManualDeskDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualDeskDraft()
    {
        var policy = Retry.DefaultPolicy("manual-desk-draft");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int ManualDeskDraftSpan(int start, int end)
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
}
