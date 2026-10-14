namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy headcount assignment.</summary>
public sealed record LegacyHeadcountAssignmentView(string Code, string Title, string UpdatedAt, string CountryText, string GraceDays, string StatusLabel)
{
    public static LegacyHeadcountAssignmentView ToLegacyHeadcountAssignmentView(LegacyHeadcountAssignment item)
    {
        return new LegacyHeadcountAssignmentView(
            item.Code,
            item.Title,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 80),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyHeadcountAssignmentView> ToLegacyHeadcountAssignmentViews(List<LegacyHeadcountAssignment> items)
    {
        var views = new List<LegacyHeadcountAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyHeadcountAssignmentView(item));
        }
        return views;
    }

    public static double SumLegacyHeadcountAssignmentView(LegacyHeadcountAssignmentView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedGraceDays;
    }
}
