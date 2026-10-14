namespace Acme.Workforce.Contractors.Offboarding.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional offboarding notice.</summary>
public sealed record RegionalOffboardingNoticeView(string Code, string Label, string Cost, string UnitText, string Description, string Country, string StatusLabel)
{
    public static RegionalOffboardingNoticeView ToRegionalOffboardingNoticeView(RegionalOffboardingNotice item)
    {
        return new RegionalOffboardingNoticeView(
            item.Code,
            item.Label,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            Text.Truncate(item.Description, 40),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalOffboardingNoticeView> ToRegionalOffboardingNoticeViews(List<RegionalOffboardingNotice> items)
    {
        var views = new List<RegionalOffboardingNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalOffboardingNoticeView(item));
        }
        return views;
    }

    public static double SumRegionalOffboardingNoticeView(RegionalOffboardingNoticeView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
