namespace Acme.Workforce.Workplace.Visitor.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global visitor draft.</summary>
public sealed record GlobalVisitorDraftView(string Code, string Title, string Rate, string Capacity, string Location, string Country, string Ceiling, string Approved, string TenureMonths, string Ratio, string EndsAt, string StatusLabel)
{
    public static GlobalVisitorDraftView ToGlobalVisitorDraftView(GlobalVisitorDraft item)
    {
        return new GlobalVisitorDraftView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 120),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Country,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalVisitorDraftView> ToGlobalVisitorDraftViews(List<GlobalVisitorDraft> items)
    {
        var views = new List<GlobalVisitorDraftView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalVisitorDraftView(item));
        }
        return views;
    }

    public static double SumGlobalVisitorDraftView(GlobalVisitorDraftView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedCapacity + parsedCeiling;
    }
}
