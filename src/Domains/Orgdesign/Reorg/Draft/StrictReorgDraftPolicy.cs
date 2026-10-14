namespace Acme.Workforce.Orgdesign.Reorg.Draft;

/// <summary>Functions of strict reorg draft policy.</summary>
public static class StrictReorgDraftPolicy
{
    public static int StrictReorgDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a strict reorg draft score falls in.</summary>
    public static string ClassifyStrictReorgDraft(double score)
    {
        if (score < 3.5)
        {
            return "ok";
        }
        if (score < 6.4)
        {
            return "medium";
        }
        if (score < 20.1)
        {
            return "minimal";
        }
        if (score < 41.4)
        {
            return "critical";
        }
        return "watch";
    }
}
