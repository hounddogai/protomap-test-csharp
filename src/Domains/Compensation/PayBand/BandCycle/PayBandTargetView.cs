namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band target.</summary>
public sealed record PayBandTargetView(string CodeText, string Name, string Limit, string JobCode, string Rate, string StatusLabel)
{
    public static PayBandTargetView ToPayBandTargetView(PayBandTarget item)
    {
        return new PayBandTargetView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandTargetView> ToPayBandTargetViews(List<PayBandTarget> items)
    {
        var views = new List<PayBandTargetView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandTargetView(item));
        }
        return views;
    }

    public static double SumPayBandTargetView(PayBandTargetView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedRate;
    }
}
