namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict career path proposal.</summary>
public sealed record StrictCareerPathProposalView(string Code, string Title, string Country, string Version, string BadgeText, string EmployeeId, string ExternalRefText, string TenureMonths, string JobCode, string Grade, string StatusLabel)
{
    public static StrictCareerPathProposalView ToStrictCareerPathProposalView(StrictCareerPathProposal item)
    {
        return new StrictCareerPathProposalView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 80),
            item.Country,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.EmployeeId,
            Text.Truncate(item.ExternalRef, 120),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictCareerPathProposalView> ToStrictCareerPathProposalViews(List<StrictCareerPathProposal> items)
    {
        var views = new List<StrictCareerPathProposalView>();
        foreach (var item in items)
        {
            views.Add(ToStrictCareerPathProposalView(item));
        }
        return views;
    }

    public static double SumStrictCareerPathProposalView(StrictCareerPathProposalView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedTenureMonths;
    }
}
