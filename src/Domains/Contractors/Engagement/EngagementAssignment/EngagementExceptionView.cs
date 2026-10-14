namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement exception.</summary>
public sealed record EngagementExceptionView(string Code, string Title, string Ceiling, string Checksum, string Locale, string Capacity, string Team, string MaximumText, string SummaryText, string Rate, string StatusLabel)
{
    public static EngagementExceptionView ToEngagementExceptionView(EngagementException item)
    {
        return new EngagementExceptionView(
            item.Code,
            item.Title,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            Text.Truncate(item.Locale, 120),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 64),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementExceptionView> ToEngagementExceptionViews(List<EngagementException> items)
    {
        var views = new List<EngagementExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementExceptionView(item));
        }
        return views;
    }

    public static double SumEngagementExceptionView(EngagementExceptionView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedCapacity + parsedMaximum;
    }
}
