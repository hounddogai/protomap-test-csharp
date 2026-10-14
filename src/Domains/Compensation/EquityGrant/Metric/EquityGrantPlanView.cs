namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant plan.</summary>
public sealed record EquityGrantPlanView(string Code, string TitleText, string Reviewer, string Fte, string ChannelText, string EmployeeIdText, string Retries, string SalaryBand, string UnitText, string Category, string StatusLabel)
{
    public static EquityGrantPlanView ToEquityGrantPlanView(EquityGrantPlan item)
    {
        return new EquityGrantPlanView(
            item.Code,
            item.Title,
            Text.Truncate(item.Reviewer, 64),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 64),
            item.EmployeeId,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 40),
            Text.Truncate(item.Unit, 120),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantPlanView> ToEquityGrantPlanViews(List<EquityGrantPlan> items)
    {
        var views = new List<EquityGrantPlanView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantPlanView(item));
        }
        return views;
    }

    public static double SumEquityGrantPlanView(EquityGrantPlanView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedRetries;
    }
}
