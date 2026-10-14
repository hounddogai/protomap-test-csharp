namespace Acme.Workforce.Talent.Pool.PoolDraft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending pool target.</summary>
public sealed record PendingPoolTargetView(string Code, string LabelText, string EmployeeIdText, string Name, string Skill, string UpdatedAt, string TitleText, string CostCenter, string Revision, string Margin, string Active, string StatusLabel)
{
    public static PendingPoolTargetView ToPendingPoolTargetView(PendingPoolTarget item)
    {
        return new PendingPoolTargetView(
            Text.Truncate(item.Code, 40),
            item.Label,
            Text.Truncate(item.EmployeeId, 40),
            item.Name,
            item.Skill,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Title,
            Text.Truncate(item.CostCenter, 40),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingPoolTargetView> ToPendingPoolTargetViews(List<PendingPoolTarget> items)
    {
        var views = new List<PendingPoolTargetView>();
        foreach (var item in items)
        {
            views.Add(ToPendingPoolTargetView(item));
        }
        return views;
    }

    public static double SumPendingPoolTargetView(PendingPoolTargetView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedRevision + parsedMargin;
    }
}
