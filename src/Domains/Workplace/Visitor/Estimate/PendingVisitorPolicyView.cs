namespace Acme.Workforce.Workplace.Visitor.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending visitor policy.</summary>
public sealed record PendingVisitorPolicyView(string Code, string Label, string Maximum, string Phone, string Reviewer, string Category, string ManagerId, string Country, string Ceiling, string StatusLabel)
{
    public static PendingVisitorPolicyView ToPendingVisitorPolicyView(PendingVisitorPolicy item)
    {
        return new PendingVisitorPolicyView(
            item.Code,
            item.Label,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            Text.Truncate(item.Reviewer, 64),
            item.Category,
            item.ManagerId,
            Text.Truncate(item.Country, 64),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingVisitorPolicyView> ToPendingVisitorPolicyViews(List<PendingVisitorPolicy> items)
    {
        var views = new List<PendingVisitorPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToPendingVisitorPolicyView(item));
        }
        return views;
    }

    public static double SumPendingVisitorPolicyView(PendingVisitorPolicyView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedCeiling;
    }
}
