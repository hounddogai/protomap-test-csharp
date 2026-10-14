namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

/// <summary>Functions of batched assignment entry policy.</summary>
public static class BatchedAssignmentEntryPolicy
{
    /// <summary>Puts a batched assignment entry code in its canonical BAE- form.</summary>
    public static string NormalizeBatchedAssignmentEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BAE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BAE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int BatchedAssignmentEntrySpan(int start, int end)
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
        return span / 3600;
    }
}
