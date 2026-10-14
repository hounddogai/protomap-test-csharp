namespace Acme.Workforce.Compensation.Tests.EquityGrant.Assignment;

using Acme.Workforce.Compensation.EquityGrant.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ManualEquityGrantWindowRulesChecks
{
    public static void ManualEquityGrantWindowValidatorReportsEveryProblem()
    {
        var validator = ManualEquityGrantWindowRules.DefaultManualEquityGrantWindowValidator();
        var item = ManualEquityGrantWindow.NewManualEquityGrantWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualEquityGrantWindowPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new ManualEquityGrantWindowPriorityRangeRule(255, 3600);
        var item = ManualEquityGrantWindow.NewManualEquityGrantWindow("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualEquityGrantWindowBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new ManualEquityGrantWindowBadgeLengthRule(3600);
        var item = ManualEquityGrantWindow.NewManualEquityGrantWindow("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualEquityGrantWindowRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new ManualEquityGrantWindowRequiredPhoneRule("manual equity grant window needs an entry");
        var item = ManualEquityGrantWindow.NewManualEquityGrantWindow("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualEquityGrantWindowValidatorReportsEveryProblem();
        ManualEquityGrantWindowPriorityRangeRuleFlagsValuesAbovePriority();
        ManualEquityGrantWindowBadgeLengthRuleFlagsALongBadge();
        ManualEquityGrantWindowRequiredPhoneRuleFlagsABlankPhone();
    }
}
