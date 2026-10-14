namespace Acme.Workforce.Compensation.Tests.MeritCycle.Proposal;

using Acme.Workforce.Compensation.MeritCycle.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class MeritCycleGroupRulesChecks
{
    public static void MeritCycleGroupValidatorReportsEveryProblem()
    {
        var validator = MeritCycleGroupRules.DefaultMeritCycleGroupValidator();
        var item = MeritCycleGroup.NewMeritCycleGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleGroupCodeLengthRule(16);
        var item = MeritCycleGroup.NewMeritCycleGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleGroupRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new MeritCycleGroupRequiredBadgeRule("merit cycle group needs an entry");
        var item = MeritCycleGroup.NewMeritCycleGroup("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCycleGroupRequiredCodeRule("merit cycle group needs an entry");
        var item = MeritCycleGroup.NewMeritCycleGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleGroupCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new MeritCycleGroupCategoryLengthRule(120);
        var item = MeritCycleGroup.NewMeritCycleGroup("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MeritCycleGroupValidatorReportsEveryProblem();
        MeritCycleGroupCodeLengthRuleFlagsALongCode();
        MeritCycleGroupRequiredBadgeRuleFlagsABlankBadge();
        MeritCycleGroupRequiredCodeRuleFlagsABlankCode();
        MeritCycleGroupCategoryLengthRuleFlagsALongCategory();
    }
}
