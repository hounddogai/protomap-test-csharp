namespace Acme.Workforce.Engagement.Feedback.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient feedback rule.</summary>
public sealed record LenientFeedbackRuleView(string Code, string LabelText, string Timezone, string ChannelText, string WindowDaysText, string StatusLabel)
{
    public static LenientFeedbackRuleView ToLenientFeedbackRuleView(LenientFeedbackRule item)
    {
        return new LenientFeedbackRuleView(
            item.Code,
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Timezone, 64),
            item.Channel,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientFeedbackRuleView> ToLenientFeedbackRuleViews(List<LenientFeedbackRule> items)
    {
        var views = new List<LenientFeedbackRuleView>();
        foreach (var item in items)
        {
            views.Add(ToLenientFeedbackRuleView(item));
        }
        return views;
    }

    public static double SumLenientFeedbackRuleView(LenientFeedbackRuleView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays;
    }
}
