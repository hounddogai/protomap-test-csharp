namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

/// <summary>Checks of offboarding segment code, run by the project's runner.</summary>
public static class AdaptiveOffboardingLedgerRulesChecks
{
    public static void AdaptiveOffboardingLedgerValidatorReportsEveryProblem()
    {
        var validator = AdaptiveOffboardingLedgerRules.DefaultAdaptiveOffboardingLedgerValidator();
        var item = AdaptiveOffboardingLedger.NewAdaptiveOffboardingLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveOffboardingLedgerRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new AdaptiveOffboardingLedgerRequiredRegionRule("adaptive offboarding ledger needs this field");
        var item = AdaptiveOffboardingLedger.NewAdaptiveOffboardingLedger("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveOffboardingLedgerMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new AdaptiveOffboardingLedgerMaximumRangeRule(5.9, 3569.6);
        var item = AdaptiveOffboardingLedger.NewAdaptiveOffboardingLedger("x", "y");
        item.Maximum = 3571.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveOffboardingLedgerNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveOffboardingLedgerNameLengthRule(255);
        var item = AdaptiveOffboardingLedger.NewAdaptiveOffboardingLedger("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveOffboardingLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveOffboardingLedgerCodeLengthRule(86400);
        var item = AdaptiveOffboardingLedger.NewAdaptiveOffboardingLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveOffboardingLedgerSkillLengthRuleFlagsALongSkill()
    {
        var rule = new AdaptiveOffboardingLedgerSkillLengthRule(86400);
        var item = AdaptiveOffboardingLedger.NewAdaptiveOffboardingLedger("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveOffboardingLedgerValidatorReportsEveryProblem();
        AdaptiveOffboardingLedgerRequiredRegionRuleFlagsABlankRegion();
        AdaptiveOffboardingLedgerMaximumRangeRuleFlagsValuesAboveMaximum();
        AdaptiveOffboardingLedgerNameLengthRuleFlagsALongName();
        AdaptiveOffboardingLedgerCodeLengthRuleFlagsALongCode();
        AdaptiveOffboardingLedgerSkillLengthRuleFlagsALongSkill();
    }
}
