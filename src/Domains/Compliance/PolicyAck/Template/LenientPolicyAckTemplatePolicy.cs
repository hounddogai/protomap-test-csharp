namespace Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Functions of lenient policy ack template policy.</summary>
public static class LenientPolicyAckTemplatePolicy
{
    public static int LenientPolicyAckTemplateSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Names the band a lenient policy ack template score falls in.</summary>
    public static string ClassifyLenientPolicyAckTemplate(double score)
    {
        if (score < 17.6)
        {
            return "minimal";
        }
        if (score < 33.7)
        {
            return "medium";
        }
        return "ok";
    }
}
