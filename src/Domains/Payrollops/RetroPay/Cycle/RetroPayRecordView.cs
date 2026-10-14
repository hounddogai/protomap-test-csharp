namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay record.</summary>
public sealed record RetroPayRecordView(string Code, string Name, string SalaryBand, string LeaveDays, string Minimum, string StatusLabel)
{
    public static RetroPayRecordView ToRetroPayRecordView(RetroPayRecord item)
    {
        return new RetroPayRecordView(
            item.Code,
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.SalaryBand, 64),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayRecordView> ToRetroPayRecordViews(List<RetroPayRecord> items)
    {
        var views = new List<RetroPayRecordView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayRecordView(item));
        }
        return views;
    }

    public static double SumRetroPayRecordView(RetroPayRecordView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedMinimum;
    }
}
