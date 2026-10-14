namespace Acme.Workforce.Absence.Policy.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback policy rule.</summary>
public sealed record FallbackPolicyRuleView(string Code, string Title, string FloorValue, string MaximumText, string Grade, string ContactEmailText, string RateText, string StatusLabel)
{
    public static FallbackPolicyRuleView ToFallbackPolicyRuleView(FallbackPolicyRule item)
    {
        return new FallbackPolicyRuleView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.ContactEmail,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPolicyRuleView> ToFallbackPolicyRuleViews(List<FallbackPolicyRule> items)
    {
        var views = new List<FallbackPolicyRuleView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPolicyRuleView(item));
        }
        return views;
    }

    public static double SumFallbackPolicyRuleView(FallbackPolicyRuleView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedMaximum + parsedRate;
    }
}
