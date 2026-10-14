namespace Acme.Workforce.Compliance.Exception.Checkpoint;

using System.Globalization;

/// <summary>How screens and exports show a exception quota.</summary>
public sealed record ExceptionQuotaView(string CodeText, string Label, string CountryText, string EndsAtText, string StatusLabel)
{
    public static ExceptionQuotaView ToExceptionQuotaView(ExceptionQuota item)
    {
        return new ExceptionQuotaView(
            item.Code,
            item.Label,
            item.Country,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionQuotaView> ToExceptionQuotaViews(List<ExceptionQuota> items)
    {
        var views = new List<ExceptionQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionQuotaView(item));
        }
        return views;
    }

    public static double SumExceptionQuotaView(ExceptionQuotaView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt;
    }
}
