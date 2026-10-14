namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending offboarding cycle.</summary>
public sealed record PendingOffboardingCycleView(string Code, string LabelText, string AmountText, string Score, string Budget, string Rate, string Name, string ManagerIdText, string Description, string Channel, string StatusLabel)
{
    public static PendingOffboardingCycleView ToPendingOffboardingCycleView(PendingOffboardingCycle item)
    {
        return new PendingOffboardingCycleView(
            item.Code,
            item.Label,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 120),
            item.ManagerId,
            item.Description,
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingOffboardingCycleView> ToPendingOffboardingCycleViews(List<PendingOffboardingCycle> items)
    {
        var views = new List<PendingOffboardingCycleView>();
        foreach (var item in items)
        {
            views.Add(ToPendingOffboardingCycleView(item));
        }
        return views;
    }

    public static double SumPendingOffboardingCycleView(PendingOffboardingCycleView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedAmount + parsedScore + parsedBudget;
    }
}
