namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global equity grant draft.</summary>
public sealed record GlobalEquityGrantDraftView(string CodeText, string TitleText, string Retries, string Region, string Team, string StatusLabel)
{
    public static GlobalEquityGrantDraftView ToGlobalEquityGrantDraftView(GlobalEquityGrantDraft item)
    {
        return new GlobalEquityGrantDraftView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 40),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalEquityGrantDraftView> ToGlobalEquityGrantDraftViews(List<GlobalEquityGrantDraft> items)
    {
        var views = new List<GlobalEquityGrantDraftView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalEquityGrantDraftView(item));
        }
        return views;
    }

    public static double SumGlobalEquityGrantDraftView(GlobalEquityGrantDraftView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
