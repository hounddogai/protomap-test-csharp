namespace Acme.Workforce.Payrollops.RetroPay.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay claim.</summary>
public sealed record RetroPayClaimView(string Code, string Name, string Source, string Checksum, string DepartmentText, string Ratio, string Ceiling, string Rate, string StatusLabel)
{
    public static RetroPayClaimView ToRetroPayClaimView(RetroPayClaim item)
    {
        return new RetroPayClaimView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Source,
            item.Checksum,
            item.Department,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayClaimView> ToRetroPayClaimViews(List<RetroPayClaim> items)
    {
        var views = new List<RetroPayClaimView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayClaimView(item));
        }
        return views;
    }

    public static double SumRetroPayClaimView(RetroPayClaimView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRatio + parsedCeiling + parsedRate;
    }
}
