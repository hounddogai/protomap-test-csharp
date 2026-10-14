namespace Acme.Workforce.Compensation.PayBand.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band bundle.</summary>
public sealed record PayBandBundleView(string CodeText, string NameText, string Attempts, string EmployeeId, string EndsAtText, string Weight, string StatusLabel)
{
    public static PayBandBundleView ToPayBandBundleView(PayBandBundle item)
    {
        return new PayBandBundleView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 120),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandBundleView> ToPayBandBundleViews(List<PayBandBundle> items)
    {
        var views = new List<PayBandBundleView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandBundleView(item));
        }
        return views;
    }

    public static double SumPayBandBundleView(PayBandBundleView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedEndsAt + parsedWeight;
    }
}
