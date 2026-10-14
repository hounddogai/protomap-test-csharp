namespace Acme.Workforce.Talent.Pool.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped pool audit.</summary>
public sealed record CappedPoolAuditView(string CodeText, string Label, string StartsAt, string Reviewer, string Retries, string Budget, string StatusLabel)
{
    public static CappedPoolAuditView ToCappedPoolAuditView(CappedPoolAudit item)
    {
        return new CappedPoolAuditView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 40),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedPoolAuditView> ToCappedPoolAuditViews(List<CappedPoolAudit> items)
    {
        var views = new List<CappedPoolAuditView>();
        foreach (var item in items)
        {
            views.Add(ToCappedPoolAuditView(item));
        }
        return views;
    }

    public static double SumCappedPoolAuditView(CappedPoolAuditView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedRetries + parsedBudget;
    }
}
