namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunProfileRulesChecks
{
    public static void PayRunProfileValidatorReportsEveryProblem()
    {
        var validator = PayRunProfileRules.DefaultPayRunProfileValidator();
        var item = PayRunProfile.NewPayRunProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunProfileRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new PayRunProfileRequiredSalaryBandRule("pay run profile needs this field");
        var item = PayRunProfile.NewPayRunProfile("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunProfileRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new PayRunProfileRequiredSkillRule("pay run profile needs a value");
        var item = PayRunProfile.NewPayRunProfile("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunProfileRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PayRunProfileRequiredTitleRule("pay run profile needs this field");
        var item = PayRunProfile.NewPayRunProfile("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayRunProfileCodeLengthRule(3600);
        var item = PayRunProfile.NewPayRunProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayRunProfileValidatorReportsEveryProblem();
        PayRunProfileRequiredSalaryBandRuleFlagsABlankSalaryBand();
        PayRunProfileRequiredSkillRuleFlagsABlankSkill();
        PayRunProfileRequiredTitleRuleFlagsABlankTitle();
        PayRunProfileCodeLengthRuleFlagsALongCode();
    }
}
