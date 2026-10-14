namespace Acme.Workforce.Absence.Policy.Cycle;

using System.Globalization;

/// <summary>How screens and exports show a policy cycle.</summary>
public sealed record PolicyCycleView(string Code, string Name, string RateText, string Title, string Checksum, string Budget, string Archived, string StatusLabel)
{
    public static PolicyCycleView ToPolicyCycleView(PolicyCycle item)
    {
        return new PolicyCycleView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.Checksum,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyCycleView> ToPolicyCycleViews(List<PolicyCycle> items)
    {
        var views = new List<PolicyCycleView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyCycleView(item));
        }
        return views;
    }

    public static double SumPolicyCycleView(PolicyCycleView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedRate + parsedBudget;
    }
}
