namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive garnishment plan.</summary>
public sealed record AdaptiveGarnishmentPlanView(string Code, string Title, string Region, string EmployeeIdText, string NotesText, string Maximum, string Budget, string Fte, string Attempts, string Badge, string StatusLabel)
{
    public static AdaptiveGarnishmentPlanView ToAdaptiveGarnishmentPlanView(AdaptiveGarnishmentPlan item)
    {
        return new AdaptiveGarnishmentPlanView(
            item.Code,
            item.Title,
            item.Region,
            item.EmployeeId,
            Text.Truncate(item.Notes, 40),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveGarnishmentPlanView> ToAdaptiveGarnishmentPlanViews(List<AdaptiveGarnishmentPlan> items)
    {
        var views = new List<AdaptiveGarnishmentPlanView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveGarnishmentPlanView(item));
        }
        return views;
    }

    public static double SumAdaptiveGarnishmentPlanView(AdaptiveGarnishmentPlanView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedBudget + parsedFte;
    }
}
