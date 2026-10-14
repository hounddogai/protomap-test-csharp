namespace Acme.Workforce.Compliance.PolicyAck.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient policy ack template.</summary>
public sealed record LenientPolicyAckTemplateView(string Code, string TitleText, string DurationMinutes, string NameText, string ExternalRef, string ThresholdText, string Sequence, string StatusLabel)
{
    public static LenientPolicyAckTemplateView ToLenientPolicyAckTemplateView(LenientPolicyAckTemplate item)
    {
        return new LenientPolicyAckTemplateView(
            item.Code,
            item.Title,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 40),
            item.ExternalRef,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientPolicyAckTemplateView> ToLenientPolicyAckTemplateViews(List<LenientPolicyAckTemplate> items)
    {
        var views = new List<LenientPolicyAckTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToLenientPolicyAckTemplateView(item));
        }
        return views;
    }

    public static double SumLenientPolicyAckTemplateView(LenientPolicyAckTemplateView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedThreshold + (double)parsedSequence;
    }
}
