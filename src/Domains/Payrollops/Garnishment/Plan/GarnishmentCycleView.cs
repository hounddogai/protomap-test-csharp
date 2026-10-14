namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment cycle.</summary>
public sealed record GarnishmentCycleView(string Code, string Name, string JobCode, string Unit, string PriorityText, string Approved, string Summary, string Team, string TenureMonthsText, string StatusLabel)
{
    public static GarnishmentCycleView ToGarnishmentCycleView(GarnishmentCycle item)
    {
        return new GarnishmentCycleView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.JobCode,
            item.Unit,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            Text.Truncate(item.Summary, 64),
            Text.Truncate(item.Team, 80),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentCycleView> ToGarnishmentCycleViews(List<GarnishmentCycle> items)
    {
        var views = new List<GarnishmentCycleView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentCycleView(item));
        }
        return views;
    }

    public static double SumGarnishmentCycleView(GarnishmentCycleView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedTenureMonths;
    }
}
