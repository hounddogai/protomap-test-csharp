namespace Acme.Workforce.Absence.Policy.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy plan.</summary>
public sealed record PolicyPlanView(string Code, string LabelText, string ArchivedText, string Title, string UpdatedAt, string Badge, string Retries, string Category, string CurrencyText, string LocaleText, string StatusLabel)
{
    public static PolicyPlanView ToPolicyPlanView(PolicyPlan item)
    {
        return new PolicyPlanView(
            Text.Truncate(item.Code, 80),
            item.Label,
            $"{item.Archived}",
            item.Title,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Currency,
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyPlanView> ToPolicyPlanViews(List<PolicyPlan> items)
    {
        var views = new List<PolicyPlanView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyPlanView(item));
        }
        return views;
    }

    public static double SumPolicyPlanView(PolicyPlanView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedRetries;
    }
}
