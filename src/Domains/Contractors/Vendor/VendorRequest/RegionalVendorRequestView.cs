namespace Acme.Workforce.Contractors.Vendor.VendorRequest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional vendor request.</summary>
public sealed record RegionalVendorRequestView(string Code, string Title, string Approved, string LocationText, string SkillText, string Country, string SalaryBand, string Score, string StatusLabel)
{
    public static RegionalVendorRequestView ToRegionalVendorRequestView(RegionalVendorRequest item)
    {
        return new RegionalVendorRequestView(
            item.Code,
            Text.Truncate(item.Title, 80),
            $"{item.Approved}",
            item.Location,
            Text.Truncate(item.Skill, 120),
            item.Country,
            item.SalaryBand,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalVendorRequestView> ToRegionalVendorRequestViews(List<RegionalVendorRequest> items)
    {
        var views = new List<RegionalVendorRequestView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalVendorRequestView(item));
        }
        return views;
    }

    public static double SumRegionalVendorRequestView(RegionalVendorRequestView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
