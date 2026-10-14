namespace Acme.Workforce.Absence.Policy.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy entry.</summary>
public sealed record PolicyEntryView(string Code, string Name, string SalaryBand, string LabelText, string Archived, string Budget, string StatusLabel)
{
    public static PolicyEntryView ToPolicyEntryView(PolicyEntry item)
    {
        return new PolicyEntryView(
            Text.Truncate(item.Code, 64),
            item.Name,
            Text.Truncate(item.SalaryBand, 40),
            item.Label,
            $"{item.Archived}",
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyEntryView> ToPolicyEntryViews(List<PolicyEntry> items)
    {
        var views = new List<PolicyEntryView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyEntryView(item));
        }
        return views;
    }

    public static double SumPolicyEntryView(PolicyEntryView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
