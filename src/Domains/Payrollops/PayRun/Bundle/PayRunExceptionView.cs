namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run exception.</summary>
public sealed record PayRunExceptionView(string Code, string Label, string Notes, string JobCode, string Category, string ManagerId, string Channel, string Title, string CostCenterText, string DurationMinutesText, string Minimum, string StatusLabel)
{
    public static PayRunExceptionView ToPayRunExceptionView(PayRunException item)
    {
        return new PayRunExceptionView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Notes,
            item.JobCode,
            item.Category,
            Text.Truncate(item.ManagerId, 40),
            item.Channel,
            item.Title,
            item.CostCenter,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunExceptionView> ToPayRunExceptionViews(List<PayRunException> items)
    {
        var views = new List<PayRunExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunExceptionView(item));
        }
        return views;
    }

    public static double SumPayRunExceptionView(PayRunExceptionView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedMinimum;
    }
}
