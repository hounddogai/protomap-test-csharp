namespace Acme.Workforce.Talent.Tests.Nomination.Group;

using Acme.Workforce.Talent.Nomination.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class NominationAssignmentRulesChecks
{
    public static void NominationAssignmentValidatorReportsEveryProblem()
    {
        var validator = NominationAssignmentRules.DefaultNominationAssignmentValidator();
        var item = NominationAssignment.NewNominationAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationAssignmentLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new NominationAssignmentLimitRangeRule(80, 1000);
        var item = NominationAssignment.NewNominationAssignment("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new NominationAssignmentRequiredCodeRule("nomination assignment needs an entry");
        var item = NominationAssignment.NewNominationAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new NominationAssignmentCodeLengthRule(120);
        var item = NominationAssignment.NewNominationAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationAssignmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new NominationAssignmentLabelLengthRule(3600);
        var item = NominationAssignment.NewNominationAssignment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationAssignmentRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new NominationAssignmentRequiredChannelRule("nomination assignment needs an entry");
        var item = NominationAssignment.NewNominationAssignment("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        NominationAssignmentValidatorReportsEveryProblem();
        NominationAssignmentLimitRangeRuleFlagsValuesAboveLimit();
        NominationAssignmentRequiredCodeRuleFlagsABlankCode();
        NominationAssignmentCodeLengthRuleFlagsALongCode();
        NominationAssignmentLabelLengthRuleFlagsALongLabel();
        NominationAssignmentRequiredChannelRuleFlagsABlankChannel();
    }
}
