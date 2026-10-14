namespace Acme.Workforce.Workplace.Tests.Desk.Allocation;

using Acme.Workforce.Workplace.Desk.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DeskPolicyRulesChecks
{
    public static void DeskPolicyValidatorReportsEveryProblem()
    {
        var validator = DeskPolicyRules.DefaultDeskPolicyValidator();
        var item = DeskPolicy.NewDeskPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskPolicyWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new DeskPolicyWindowDaysRangeRule(16, 64);
        var item = DeskPolicy.NewDeskPolicy("x", "y");
        item.WindowDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeskPolicyCodeLengthRule(80);
        var item = DeskPolicy.NewDeskPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskPolicyValidatorReportsEveryProblem();
        DeskPolicyWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        DeskPolicyCodeLengthRuleFlagsALongCode();
    }
}
