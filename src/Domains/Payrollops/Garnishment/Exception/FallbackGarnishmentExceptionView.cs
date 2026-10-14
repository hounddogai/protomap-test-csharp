namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback garnishment exception.</summary>
public sealed record FallbackGarnishmentExceptionView(string CodeText, string NameText, string RegionText, string Unit, string AmountText, string Minimum, string Visible, string Limit, string StatusLabel)
{
    public static FallbackGarnishmentExceptionView ToFallbackGarnishmentExceptionView(FallbackGarnishmentException item)
    {
        return new FallbackGarnishmentExceptionView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Region,
            Text.Truncate(item.Unit, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackGarnishmentExceptionView> ToFallbackGarnishmentExceptionViews(List<FallbackGarnishmentException> items)
    {
        var views = new List<FallbackGarnishmentExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackGarnishmentExceptionView(item));
        }
        return views;
    }

    public static double SumFallbackGarnishmentExceptionView(FallbackGarnishmentExceptionView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedAmount + parsedMinimum + (double)parsedLimit;
    }
}
