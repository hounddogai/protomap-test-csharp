namespace Acme.Workforce.Talent.Pool.Exception;

using System.Globalization;

/// <summary>How screens and exports show a pool profile.</summary>
public sealed record PoolProfileView(string CodeText, string Label, string Maximum, string Summary, string Source, string CreatedAt, string NotesText, string ContactEmailText, string StatusLabel)
{
    public static PoolProfileView ToPoolProfileView(PoolProfile item)
    {
        return new PoolProfileView(
            item.Code,
            item.Label,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Source,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolProfileView> ToPoolProfileViews(List<PoolProfile> items)
    {
        var views = new List<PoolProfileView>();
        foreach (var item in items)
        {
            views.Add(ToPoolProfileView(item));
        }
        return views;
    }

    public static double SumPoolProfileView(PoolProfileView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedCreatedAt;
    }
}
