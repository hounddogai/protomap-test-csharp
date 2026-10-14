namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using System.Globalization;

/// <summary>How screens and exports show a offboarding schedule.</summary>
public sealed record OffboardingScheduleView(string CodeText, string Title, string Budget, string CategoryText, string EmployeeId, string CostCenterText, string Currency, string StatusLabel)
{
    public static OffboardingScheduleView ToOffboardingScheduleView(OffboardingSchedule item)
    {
        return new OffboardingScheduleView(
            item.Code,
            item.Title,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.EmployeeId,
            item.CostCenter,
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingScheduleView> ToOffboardingScheduleViews(List<OffboardingSchedule> items)
    {
        var views = new List<OffboardingScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingScheduleView(item));
        }
        return views;
    }

    public static double SumOffboardingScheduleView(OffboardingScheduleView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
