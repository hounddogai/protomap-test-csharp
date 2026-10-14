namespace Acme.Workforce.Engagement.Recognition.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped recognition rule.</summary>
public sealed record CappedRecognitionRuleView(string Code, string TitleText, string PhoneText, string Category, string EndsAtText, string Badge, string StatusLabel)
{
    public static CappedRecognitionRuleView ToCappedRecognitionRuleView(CappedRecognitionRule item)
    {
        return new CappedRecognitionRuleView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Phone,
            item.Category,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedRecognitionRuleView> ToCappedRecognitionRuleViews(List<CappedRecognitionRule> items)
    {
        var views = new List<CappedRecognitionRuleView>();
        foreach (var item in items)
        {
            views.Add(ToCappedRecognitionRuleView(item));
        }
        return views;
    }

    public static double SumCappedRecognitionRuleView(CappedRecognitionRuleView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt;
    }
}
