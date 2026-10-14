namespace Acme.Workforce.Assets.AssetReturn.Summary;

using System.Globalization;

/// <summary>How screens and exports show a scheduled asset return proposal.</summary>
public sealed record ScheduledAssetReturnProposalView(string Code, string NameText, string EmployeeId, string DepartmentText, string VersionText, string VisibleText, string Grade, string Region, string Approved, string StatusLabel)
{
    public static ScheduledAssetReturnProposalView ToScheduledAssetReturnProposalView(ScheduledAssetReturnProposal item)
    {
        return new ScheduledAssetReturnProposalView(
            item.Code,
            item.Name,
            item.EmployeeId,
            item.Department,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Grade,
            item.Region,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledAssetReturnProposalView> ToScheduledAssetReturnProposalViews(List<ScheduledAssetReturnProposal> items)
    {
        var views = new List<ScheduledAssetReturnProposalView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledAssetReturnProposalView(item));
        }
        return views;
    }

    public static double SumScheduledAssetReturnProposalView(ScheduledAssetReturnProposalView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return (double)parsedVersion;
    }
}
