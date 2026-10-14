namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using System.Globalization;

/// <summary>How screens and exports show a primary garnishment mapping.</summary>
public sealed record PrimaryGarnishmentMappingView(string Code, string Name, string CeilingText, string UpdatedAtText, string Score, string ExternalRef, string EmployeeId, string StatusLabel)
{
    public static PrimaryGarnishmentMappingView ToPrimaryGarnishmentMappingView(PrimaryGarnishmentMapping item)
    {
        return new PrimaryGarnishmentMappingView(
            item.Code,
            item.Name,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryGarnishmentMappingView> ToPrimaryGarnishmentMappingViews(List<PrimaryGarnishmentMapping> items)
    {
        var views = new List<PrimaryGarnishmentMappingView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryGarnishmentMappingView(item));
        }
        return views;
    }

    public static double SumPrimaryGarnishmentMappingView(PrimaryGarnishmentMappingView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedUpdatedAt + parsedScore;
    }
}
