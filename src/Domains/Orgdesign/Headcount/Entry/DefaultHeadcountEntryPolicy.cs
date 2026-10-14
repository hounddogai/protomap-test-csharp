namespace Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>Functions of default headcount entry policy.</summary>
public static class DefaultHeadcountEntryPolicy
{
    /// <summary>Names the band a default headcount entry score falls in.</summary>
    public static string ClassifyDefaultHeadcountEntry(double score)
    {
        if (score < 35.8)
        {
            return "watch";
        }
        if (score < 60.5)
        {
            return "high";
        }
        if (score < 70.0)
        {
            return "low";
        }
        if (score < 84.6)
        {
            return "critical";
        }
        return "ok";
    }

    public static int DefaultHeadcountEntrySpan(int start, int end)
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
