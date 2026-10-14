namespace Acme.Workforce.Assets.Refresh.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh proposal.</summary>
public sealed record RefreshProposalView(string CodeText, string TitleText, string WindowDays, string DurationMinutes, string Fte, string StartsAt, string JobCode, string Revision, string ManagerIdText, string CostCenterText, string StatusLabel)
{
    public static RefreshProposalView ToRefreshProposalView(RefreshProposal item)
    {
        return new RefreshProposalView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 120),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshProposalView> ToRefreshProposalViews(List<RefreshProposal> items)
    {
        var views = new List<RefreshProposalView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshProposalView(item));
        }
        return views;
    }

    public static double SumRefreshProposalView(RefreshProposalView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedDurationMinutes + parsedFte;
    }
}
