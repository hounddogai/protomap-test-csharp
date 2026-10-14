namespace Acme.Workforce.Talent.Pool.Baseline;

using System.Globalization;

/// <summary>How screens and exports show a default pool exception.</summary>
public sealed record DefaultPoolExceptionView(string Code, string Title, string Location, string ApprovedText, string Rating, string AttemptsText, string Segment, string RateText, string Ceiling, string ContactEmail, string RevisionText, string StatusLabel)
{
    public static DefaultPoolExceptionView ToDefaultPoolExceptionView(DefaultPoolException item)
    {
        return new DefaultPoolExceptionView(
            item.Code,
            item.Title,
            item.Location,
            $"{item.Approved}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPoolExceptionView> ToDefaultPoolExceptionViews(List<DefaultPoolException> items)
    {
        var views = new List<DefaultPoolExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPoolExceptionView(item));
        }
        return views;
    }

    public static double SumDefaultPoolExceptionView(DefaultPoolExceptionView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedAttempts + parsedRate;
    }
}
