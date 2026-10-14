namespace Acme.Workforce.Compensation.EquityGrant.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant notice.</summary>
public sealed record EquityGrantNoticeView(string CodeText, string Title, string Capacity, string Timezone, string VersionText, string Budget, string StatusLabel)
{
    public static EquityGrantNoticeView ToEquityGrantNoticeView(EquityGrantNotice item)
    {
        return new EquityGrantNoticeView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantNoticeView> ToEquityGrantNoticeViews(List<EquityGrantNotice> items)
    {
        var views = new List<EquityGrantNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantNoticeView(item));
        }
        return views;
    }

    public static double SumEquityGrantNoticeView(EquityGrantNoticeView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedVersion + parsedBudget;
    }
}
