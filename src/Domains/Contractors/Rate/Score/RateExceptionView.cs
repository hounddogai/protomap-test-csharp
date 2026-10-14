namespace Acme.Workforce.Contractors.Rate.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate exception.</summary>
public sealed record RateExceptionView(string Code, string Title, string Summary, string NotesText, string Rating, string StatusLabel)
{
    public static RateExceptionView ToRateExceptionView(RateException item)
    {
        return new RateExceptionView(
            item.Code,
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.Summary, 64),
            item.Notes,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateExceptionView> ToRateExceptionViews(List<RateException> items)
    {
        var views = new List<RateExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToRateExceptionView(item));
        }
        return views;
    }

    public static double SumRateExceptionView(RateExceptionView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedRating;
    }
}
