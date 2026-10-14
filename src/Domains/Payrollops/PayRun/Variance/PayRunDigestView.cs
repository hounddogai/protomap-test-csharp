namespace Acme.Workforce.Payrollops.PayRun.Variance;

using System.Globalization;

/// <summary>How screens and exports show a pay run digest.</summary>
public sealed record PayRunDigestView(string CodeText, string TitleText, string GraceDaysText, string Fte, string WindowDays, string Capacity, string Label, string Amount, string StatusLabel)
{
    public static PayRunDigestView ToPayRunDigestView(PayRunDigest item)
    {
        return new PayRunDigestView(
            item.Code,
            item.Title,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunDigestView> ToPayRunDigestViews(List<PayRunDigest> items)
    {
        var views = new List<PayRunDigestView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunDigestView(item));
        }
        return views;
    }

    public static double SumPayRunDigestView(PayRunDigestView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedFte + (double)parsedWindowDays;
    }
}
