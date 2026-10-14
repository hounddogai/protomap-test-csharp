namespace Acme.Workforce.Workplace.Desk.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default desk policy.</summary>
public sealed record DefaultDeskPolicyView(string CodeText, string TitleText, string Locale, string ReviewerText, string Retries, string StatusLabel)
{
    public static DefaultDeskPolicyView ToDefaultDeskPolicyView(DefaultDeskPolicy item)
    {
        return new DefaultDeskPolicyView(
            Text.Truncate(item.Code, 40),
            item.Title,
            Text.Truncate(item.Locale, 120),
            item.Reviewer,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultDeskPolicyView> ToDefaultDeskPolicyViews(List<DefaultDeskPolicy> items)
    {
        var views = new List<DefaultDeskPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultDeskPolicyView(item));
        }
        return views;
    }

    public static double SumDefaultDeskPolicyView(DefaultDeskPolicyView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
