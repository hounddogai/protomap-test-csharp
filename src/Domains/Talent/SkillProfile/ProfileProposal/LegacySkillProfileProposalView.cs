namespace Acme.Workforce.Talent.SkillProfile.ProfileProposal;

using System.Globalization;

/// <summary>How screens and exports show a legacy skill profile proposal.</summary>
public sealed record LegacySkillProfileProposalView(string Code, string Title, string EmployeeId, string Description, string BudgetText, string Maximum, string Capacity, string StatusLabel)
{
    public static LegacySkillProfileProposalView ToLegacySkillProfileProposalView(LegacySkillProfileProposal item)
    {
        return new LegacySkillProfileProposalView(
            item.Code,
            item.Title,
            item.EmployeeId,
            item.Description,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacySkillProfileProposalView> ToLegacySkillProfileProposalViews(List<LegacySkillProfileProposal> items)
    {
        var views = new List<LegacySkillProfileProposalView>();
        foreach (var item in items)
        {
            views.Add(ToLegacySkillProfileProposalView(item));
        }
        return views;
    }

    public static double SumLegacySkillProfileProposalView(LegacySkillProfileProposalView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedBudget + parsedMaximum + (double)parsedCapacity;
    }
}
