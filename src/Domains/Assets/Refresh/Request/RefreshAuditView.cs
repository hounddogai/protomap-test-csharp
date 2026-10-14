namespace Acme.Workforce.Assets.Refresh.Request;

using System.Globalization;

/// <summary>How screens and exports show a refresh audit.</summary>
public sealed record RefreshAuditView(string Code, string TitleText, string Hours, string CurrencyText, string SummaryText, string StatusLabel)
{
    public static RefreshAuditView ToRefreshAuditView(RefreshAudit item)
    {
        return new RefreshAuditView(
            item.Code,
            item.Title,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshAuditView> ToRefreshAuditViews(List<RefreshAudit> items)
    {
        var views = new List<RefreshAuditView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshAuditView(item));
        }
        return views;
    }

    public static double SumRefreshAuditView(RefreshAuditView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedHours;
    }
}
