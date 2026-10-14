namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using System.Globalization;

/// <summary>How screens and exports show a exception summary.</summary>
public sealed record ExceptionSummaryView(string Code, string NameText, string Discount, string Team, string ContactEmailText, string Notes, string CapacityText, string StatusLabel)
{
    public static ExceptionSummaryView ToExceptionSummaryView(ExceptionSummary item)
    {
        return new ExceptionSummaryView(
            item.Code,
            item.Name,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.ContactEmail,
            item.Notes,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionSummaryView> ToExceptionSummaryViews(List<ExceptionSummary> items)
    {
        var views = new List<ExceptionSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionSummaryView(item));
        }
        return views;
    }

    public static double SumExceptionSummaryView(ExceptionSummaryView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedCapacity;
    }
}
