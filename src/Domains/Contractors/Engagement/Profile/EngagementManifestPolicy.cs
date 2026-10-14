namespace Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Functions of engagement manifest policy.</summary>
public static class EngagementManifestPolicy
{
    public static int EngagementManifestSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Puts a engagement manifest code in its canonical EM- form.</summary>
    public static string NormalizeEngagementManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
