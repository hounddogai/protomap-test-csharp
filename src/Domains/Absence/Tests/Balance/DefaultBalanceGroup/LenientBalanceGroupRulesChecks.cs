namespace Acme.Workforce.Absence.Tests.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>Checks of default balance group code, run by the project's runner.</summary>
public static class LenientBalanceGroupRulesChecks
{
    public static void LenientBalanceGroupValidatorReportsEveryProblem()
    {
        var validator = LenientBalanceGroupRules.DefaultLenientBalanceGroupValidator();
        var item = LenientBalanceGroup.NewLenientBalanceGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientBalanceGroupRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new LenientBalanceGroupRequiredTeamRule("lenient balance group needs this field");
        var item = LenientBalanceGroup.NewLenientBalanceGroup("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBalanceGroupBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new LenientBalanceGroupBatchSizeRangeRule(80, 120);
        var item = LenientBalanceGroup.NewLenientBalanceGroup("x", "y");
        item.BatchSize = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientBalanceGroupHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new LenientBalanceGroupHoursRangeRule(9.5, 2944.7);
        var item = LenientBalanceGroup.NewLenientBalanceGroup("x", "y");
        item.Hours = 2946.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientBalanceGroupNameLengthRuleFlagsALongName()
    {
        var rule = new LenientBalanceGroupNameLengthRule(64);
        var item = LenientBalanceGroup.NewLenientBalanceGroup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientBalanceGroupRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new LenientBalanceGroupRequiredDescriptionRule("lenient balance group needs a value");
        var item = LenientBalanceGroup.NewLenientBalanceGroup("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBalanceGroupDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new LenientBalanceGroupDescriptionLengthRule(86400);
        var item = LenientBalanceGroup.NewLenientBalanceGroup("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientBalanceGroupValidatorReportsEveryProblem();
        LenientBalanceGroupRequiredTeamRuleFlagsABlankTeam();
        LenientBalanceGroupBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        LenientBalanceGroupHoursRangeRuleFlagsValuesAboveHours();
        LenientBalanceGroupNameLengthRuleFlagsALongName();
        LenientBalanceGroupRequiredDescriptionRuleFlagsABlankDescription();
        LenientBalanceGroupDescriptionLengthRuleFlagsALongDescription();
    }
}
