namespace Acme.Workforce.Contractors.Offboarding.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding cycle.</summary>
public sealed record OffboardingCycleView(string Code, string Name, string AmountText, string AttemptsText, string CostCenterText, string LimitText, string Threshold, string Department, string Minimum, string Hours, string StatusLabel)
{
    public static OffboardingCycleView ToOffboardingCycleView(OffboardingCycle item)
    {
        return new OffboardingCycleView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingCycleView> ToOffboardingCycleViews(List<OffboardingCycle> items)
    {
        var views = new List<OffboardingCycleView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingCycleView(item));
        }
        return views;
    }

    public static double SumOffboardingCycleView(OffboardingCycleView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedAttempts + (double)parsedLimit;
    }
}
