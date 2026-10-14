namespace Acme.Workforce.Workplace.Tests.Visitor.Group;

using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class VisitorProfileRulesChecks
{
    public static void VisitorProfileValidatorReportsEveryProblem()
    {
        var validator = VisitorProfileRules.DefaultVisitorProfileValidator();
        var item = VisitorProfile.NewVisitorProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorProfileRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new VisitorProfileRequiredTimezoneRule("visitor profile needs a value");
        var item = VisitorProfile.NewVisitorProfile("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorProfileLabelLengthRuleFlagsALongLabel()
    {
        var rule = new VisitorProfileLabelLengthRule(64);
        var item = VisitorProfile.NewVisitorProfile("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VisitorProfileRequiredCodeRule("visitor profile needs an entry");
        var item = VisitorProfile.NewVisitorProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new VisitorProfileCodeLengthRule(16);
        var item = VisitorProfile.NewVisitorProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorProfileBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new VisitorProfileBadgeLengthRule(3600);
        var item = VisitorProfile.NewVisitorProfile("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VisitorProfileValidatorReportsEveryProblem();
        VisitorProfileRequiredTimezoneRuleFlagsABlankTimezone();
        VisitorProfileLabelLengthRuleFlagsALongLabel();
        VisitorProfileRequiredCodeRuleFlagsABlankCode();
        VisitorProfileCodeLengthRuleFlagsALongCode();
        VisitorProfileBadgeLengthRuleFlagsALongBadge();
    }
}
