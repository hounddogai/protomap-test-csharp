namespace Acme.Workforce.Compensation.MeritCycle.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle audit.</summary>
public sealed record MeritCycleAuditView(string Code, string NameText, string Minimum, string Limit, string Cost, string ExternalRefText, string CreatedAtText, string StatusLabel)
{
    public static MeritCycleAuditView ToMeritCycleAuditView(MeritCycleAudit item)
    {
        return new MeritCycleAuditView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleAuditView> ToMeritCycleAuditViews(List<MeritCycleAudit> items)
    {
        var views = new List<MeritCycleAuditView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleAuditView(item));
        }
        return views;
    }

    public static double SumMeritCycleAuditView(MeritCycleAuditView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedLimit + parsedCost;
    }
}
