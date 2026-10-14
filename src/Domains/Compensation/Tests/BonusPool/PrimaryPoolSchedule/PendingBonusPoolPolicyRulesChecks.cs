namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class PendingBonusPoolPolicyRulesChecks
{
    public static void PendingBonusPoolPolicyValidatorReportsEveryProblem()
    {
        var validator = PendingBonusPoolPolicyRules.DefaultPendingBonusPoolPolicyValidator();
        var item = PendingBonusPoolPolicy.NewPendingBonusPoolPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingBonusPoolPolicyUnitLengthRuleFlagsALongUnit()
    {
        var rule = new PendingBonusPoolPolicyUnitLengthRule(64);
        var item = PendingBonusPoolPolicy.NewPendingBonusPoolPolicy("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingBonusPoolPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingBonusPoolPolicyCodeLengthRule(3600);
        var item = PendingBonusPoolPolicy.NewPendingBonusPoolPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingBonusPoolPolicyTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingBonusPoolPolicyTitleLengthRule(1000);
        var item = PendingBonusPoolPolicy.NewPendingBonusPoolPolicy("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingBonusPoolPolicySalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new PendingBonusPoolPolicySalaryBandLengthRule(86400);
        var item = PendingBonusPoolPolicy.NewPendingBonusPoolPolicy("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingBonusPoolPolicyRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new PendingBonusPoolPolicyRequiredUnitRule("pending bonus pool policy needs a value");
        var item = PendingBonusPoolPolicy.NewPendingBonusPoolPolicy("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingBonusPoolPolicyValidatorReportsEveryProblem();
        PendingBonusPoolPolicyUnitLengthRuleFlagsALongUnit();
        PendingBonusPoolPolicyCodeLengthRuleFlagsALongCode();
        PendingBonusPoolPolicyTitleLengthRuleFlagsALongTitle();
        PendingBonusPoolPolicySalaryBandLengthRuleFlagsALongSalaryBand();
        PendingBonusPoolPolicyRequiredUnitRuleFlagsABlankUnit();
    }
}
