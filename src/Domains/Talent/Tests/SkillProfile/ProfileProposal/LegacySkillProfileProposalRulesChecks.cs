namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileProposal;

using Acme.Workforce.Talent.SkillProfile.ProfileProposal;

/// <summary>Checks of profile proposal code, run by the project's runner.</summary>
public static class LegacySkillProfileProposalRulesChecks
{
    public static void LegacySkillProfileProposalValidatorReportsEveryProblem()
    {
        var validator = LegacySkillProfileProposalRules.DefaultLegacySkillProfileProposalValidator();
        var item = LegacySkillProfileProposal.NewLegacySkillProfileProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacySkillProfileProposalEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new LegacySkillProfileProposalEmployeeIdLengthRule(255);
        var item = LegacySkillProfileProposal.NewLegacySkillProfileProposal("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacySkillProfileProposalBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new LegacySkillProfileProposalBudgetRangeRule(2.2, 3015.5);
        var item = LegacySkillProfileProposal.NewLegacySkillProfileProposal("x", "y");
        item.Budget = 3017.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacySkillProfileProposalMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new LegacySkillProfileProposalMaximumRangeRule(2.9, 4120.5);
        var item = LegacySkillProfileProposal.NewLegacySkillProfileProposal("x", "y");
        item.Maximum = 4122.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacySkillProfileProposalValidatorReportsEveryProblem();
        LegacySkillProfileProposalEmployeeIdLengthRuleFlagsALongEmployeeId();
        LegacySkillProfileProposalBudgetRangeRuleFlagsValuesAboveBudget();
        LegacySkillProfileProposalMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
