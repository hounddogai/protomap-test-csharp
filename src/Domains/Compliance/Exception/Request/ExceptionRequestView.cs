namespace Acme.Workforce.Compliance.Exception.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception request.</summary>
public sealed record ExceptionRequestView(string Code, string Label, string Visible, string LocationText, string ExternalRef, string OwnerId, string Rating, string Summary, string TenureMonthsText, string StatusLabel)
{
    public static ExceptionRequestView ToExceptionRequestView(ExceptionRequest item)
    {
        return new ExceptionRequestView(
            item.Code,
            Text.Truncate(item.Label, 80),
            $"{item.Visible}",
            item.Location,
            item.ExternalRef,
            item.OwnerId,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionRequestView> ToExceptionRequestViews(List<ExceptionRequest> items)
    {
        var views = new List<ExceptionRequestView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionRequestView(item));
        }
        return views;
    }

    public static double SumExceptionRequestView(ExceptionRequestView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedTenureMonths;
    }
}
