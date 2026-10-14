namespace Acme.Workforce.Assets.Refresh.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh rule.</summary>
public sealed record RefreshRuleView(string CodeText, string Label, string OwnerId, string EmployeeIdText, string Source, string ExternalRef, string Retries, string StatusLabel)
{
    public static RefreshRuleView ToRefreshRuleView(RefreshRule item)
    {
        return new RefreshRuleView(
            item.Code,
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.OwnerId, 80),
            item.EmployeeId,
            item.Source,
            item.ExternalRef,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshRuleView> ToRefreshRuleViews(List<RefreshRule> items)
    {
        var views = new List<RefreshRuleView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshRuleView(item));
        }
        return views;
    }

    public static double SumRefreshRuleView(RefreshRuleView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
