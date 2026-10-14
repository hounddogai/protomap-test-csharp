namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy license seat proposal.</summary>
public sealed record LegacyLicenseSeatProposalView(string Code, string Name, string LimitText, string Margin, string Category, string OwnerId, string LocaleText, string Priority, string Pinned, string Ceiling, string StatusLabel)
{
    public static LegacyLicenseSeatProposalView ToLegacyLicenseSeatProposalView(LegacyLicenseSeatProposal item)
    {
        return new LegacyLicenseSeatProposalView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 40),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.OwnerId,
            item.Locale,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyLicenseSeatProposalView> ToLegacyLicenseSeatProposalViews(List<LegacyLicenseSeatProposal> items)
    {
        var views = new List<LegacyLicenseSeatProposalView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyLicenseSeatProposalView(item));
        }
        return views;
    }

    public static double SumLegacyLicenseSeatProposalView(LegacyLicenseSeatProposalView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedMargin + (double)parsedPriority;
    }
}
