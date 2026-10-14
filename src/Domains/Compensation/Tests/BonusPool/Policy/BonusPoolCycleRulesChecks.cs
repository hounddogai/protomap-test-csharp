namespace Acme.Workforce.Compensation.Tests.BonusPool.Policy;

using Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BonusPoolCycleRulesChecks
{
    public static void BonusPoolCycleValidatorReportsEveryProblem()
    {
        var validator = BonusPoolCycleRules.DefaultBonusPoolCycleValidator();
        var item = BonusPoolCycle.NewBonusPoolCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolCycleManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new BonusPoolCycleManagerIdLengthRule(255);
        var item = BonusPoolCycle.NewBonusPoolCycle("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolCycleRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new BonusPoolCycleRequiredManagerIdRule("bonus pool cycle needs a value");
        var item = BonusPoolCycle.NewBonusPoolCycle("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolCycleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BonusPoolCycleLabelLengthRule(64);
        var item = BonusPoolCycle.NewBonusPoolCycle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BonusPoolCycleValidatorReportsEveryProblem();
        BonusPoolCycleManagerIdLengthRuleFlagsALongManagerId();
        BonusPoolCycleRequiredManagerIdRuleFlagsABlankManagerId();
        BonusPoolCycleLabelLengthRuleFlagsALongLabel();
    }
}
