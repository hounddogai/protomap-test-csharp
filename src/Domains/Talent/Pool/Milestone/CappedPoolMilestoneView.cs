namespace Acme.Workforce.Talent.Pool.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped pool milestone.</summary>
public sealed record CappedPoolMilestoneView(string CodeText, string NameText, string Summary, string Country, string Capacity, string RateText, string MarginText, string EndsAt, string Version, string Notes, string StatusLabel)
{
    public static CappedPoolMilestoneView ToCappedPoolMilestoneView(CappedPoolMilestone item)
    {
        return new CappedPoolMilestoneView(
            item.Code,
            item.Name,
            Text.Truncate(item.Summary, 120),
            item.Country,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedPoolMilestoneView> ToCappedPoolMilestoneViews(List<CappedPoolMilestone> items)
    {
        var views = new List<CappedPoolMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToCappedPoolMilestoneView(item));
        }
        return views;
    }

    public static double SumCappedPoolMilestoneView(CappedPoolMilestoneView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedRate + parsedMargin;
    }
}
