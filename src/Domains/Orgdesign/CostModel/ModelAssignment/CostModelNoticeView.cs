namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a cost model notice.</summary>
public sealed record CostModelNoticeView(string Code, string TitleText, string ArchivedText, string Region, string UpdatedAtText, string HoursText, string Ceiling, string LocationText, string StatusLabel)
{
    public static CostModelNoticeView ToCostModelNoticeView(CostModelNotice item)
    {
        return new CostModelNoticeView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 80),
            $"{item.Archived}",
            item.Region,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelNoticeView> ToCostModelNoticeViews(List<CostModelNotice> items)
    {
        var views = new List<CostModelNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelNoticeView(item));
        }
        return views;
    }

    public static double SumCostModelNoticeView(CostModelNoticeView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedHours + parsedCeiling;
    }
}
