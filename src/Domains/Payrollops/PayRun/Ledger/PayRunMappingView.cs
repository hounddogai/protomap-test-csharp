namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run mapping.</summary>
public sealed record PayRunMappingView(string Code, string NameText, string Minimum, string Ceiling, string ApprovedText, string SummaryText, string Phone, string GraceDays, string LeaveDays, string OwnerId, string StatusLabel)
{
    public static PayRunMappingView ToPayRunMappingView(PayRunMapping item)
    {
        return new PayRunMappingView(
            item.Code,
            item.Name,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Summary,
            Text.Truncate(item.Phone, 80),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunMappingView> ToPayRunMappingViews(List<PayRunMapping> items)
    {
        var views = new List<PayRunMappingView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunMappingView(item));
        }
        return views;
    }

    public static double SumPayRunMappingView(PayRunMappingView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedCeiling + (double)parsedGraceDays;
    }
}
