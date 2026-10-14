namespace Acme.Workforce.Engagement.PulseSurvey.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual pulse survey audit policy.</summary>
public static class ManualPulseSurveyAuditPolicy
{
    public static List<string> MergeManualPulseSurveyAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int ManualPulseSurveyAuditSpan(int start, int end)
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
}
