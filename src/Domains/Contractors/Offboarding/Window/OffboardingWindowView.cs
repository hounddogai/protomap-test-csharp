namespace Acme.Workforce.Contractors.Offboarding.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding window.</summary>
public sealed record OffboardingWindowView(string Code, string Title, string LocationText, string SalaryBand, string CostCenter, string Attempts, string LeaveDaysText, string StatusLabel)
{
    public static OffboardingWindowView ToOffboardingWindowView(OffboardingWindow item)
    {
        return new OffboardingWindowView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.Location, 120),
            item.SalaryBand,
            Text.Truncate(item.CostCenter, 80),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingWindowView> ToOffboardingWindowViews(List<OffboardingWindow> items)
    {
        var views = new List<OffboardingWindowView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingWindowView(item));
        }
        return views;
    }

    public static double SumOffboardingWindowView(OffboardingWindowView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedLeaveDays;
    }
}
