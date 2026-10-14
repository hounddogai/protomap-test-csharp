namespace Acme.Workforce.Talent.CareerPath.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path summary.</summary>
public sealed record CareerPathSummaryView(string Code, string TitleText, string Location, string PinnedText, string ExternalRef, string Locale, string Phone, string EndsAtText, string StatusLabel)
{
    public static CareerPathSummaryView ToCareerPathSummaryView(CareerPathSummary item)
    {
        return new CareerPathSummaryView(
            item.Code,
            item.Title,
            item.Location,
            $"{item.Pinned}",
            item.ExternalRef,
            Text.Truncate(item.Locale, 80),
            item.Phone,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathSummaryView> ToCareerPathSummaryViews(List<CareerPathSummary> items)
    {
        var views = new List<CareerPathSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathSummaryView(item));
        }
        return views;
    }

    public static double SumCareerPathSummaryView(CareerPathSummaryView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt;
    }
}
