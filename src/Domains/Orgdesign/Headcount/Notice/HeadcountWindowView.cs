namespace Acme.Workforce.Orgdesign.Headcount.Notice;

using System.Globalization;

/// <summary>How screens and exports show a headcount window.</summary>
public sealed record HeadcountWindowView(string CodeText, string Label, string Archived, string LeaveDays, string Location, string SourceText, string RetriesText, string StatusLabel)
{
    public static HeadcountWindowView ToHeadcountWindowView(HeadcountWindow item)
    {
        return new HeadcountWindowView(
            item.Code,
            item.Label,
            $"{item.Archived}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Source,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountWindowView> ToHeadcountWindowViews(List<HeadcountWindow> items)
    {
        var views = new List<HeadcountWindowView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountWindowView(item));
        }
        return views;
    }

    public static double SumHeadcountWindowView(HeadcountWindowView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedRetries;
    }
}
