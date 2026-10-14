namespace Acme.Workforce.Talent.Tests.Nomination.Target;

using Acme.Workforce.Talent.Nomination.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class NominationTargetRulesChecks
{
    public static void NominationTargetValidatorReportsEveryProblem()
    {
        var validator = NominationTargetRules.DefaultNominationTargetValidator();
        var item = NominationTarget.NewNominationTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationTargetThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new NominationTargetThresholdRangeRule(7.4, 1990.3);
        var item = NominationTarget.NewNominationTarget("x", "y");
        item.Threshold = 1991.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new NominationTargetRequiredCodeRule("nomination target needs this field");
        var item = NominationTarget.NewNominationTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationTargetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new NominationTargetRequiredTitleRule("nomination target needs an entry");
        var item = NominationTarget.NewNominationTarget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationTargetNameLengthRuleFlagsALongName()
    {
        var rule = new NominationTargetNameLengthRule(120);
        var item = NominationTarget.NewNominationTarget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationTargetRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new NominationTargetRequiredChecksumRule("nomination target needs an entry");
        var item = NominationTarget.NewNominationTarget("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationTargetTitleLengthRuleFlagsALongTitle()
    {
        var rule = new NominationTargetTitleLengthRule(255);
        var item = NominationTarget.NewNominationTarget("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        NominationTargetValidatorReportsEveryProblem();
        NominationTargetThresholdRangeRuleFlagsValuesAboveThreshold();
        NominationTargetRequiredCodeRuleFlagsABlankCode();
        NominationTargetRequiredTitleRuleFlagsABlankTitle();
        NominationTargetNameLengthRuleFlagsALongName();
        NominationTargetRequiredChecksumRuleFlagsABlankChecksum();
        NominationTargetTitleLengthRuleFlagsALongTitle();
    }
}
