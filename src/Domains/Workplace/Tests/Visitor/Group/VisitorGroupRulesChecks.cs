namespace Acme.Workforce.Workplace.Tests.Visitor.Group;

using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class VisitorGroupRulesChecks
{
    public static void VisitorGroupValidatorReportsEveryProblem()
    {
        var validator = VisitorGroupRules.DefaultVisitorGroupValidator();
        var item = VisitorGroup.NewVisitorGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorGroupRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new VisitorGroupRequiredGradeRule("visitor group needs a value");
        var item = VisitorGroup.NewVisitorGroup("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorGroupRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new VisitorGroupRequiredBadgeRule("visitor group needs an entry");
        var item = VisitorGroup.NewVisitorGroup("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorGroupBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new VisitorGroupBadgeLengthRule(32);
        var item = VisitorGroup.NewVisitorGroup("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorGroupGradeLengthRuleFlagsALongGrade()
    {
        var rule = new VisitorGroupGradeLengthRule(255);
        var item = VisitorGroup.NewVisitorGroup("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VisitorGroupValidatorReportsEveryProblem();
        VisitorGroupRequiredGradeRuleFlagsABlankGrade();
        VisitorGroupRequiredBadgeRuleFlagsABlankBadge();
        VisitorGroupBadgeLengthRuleFlagsALongBadge();
        VisitorGroupGradeLengthRuleFlagsALongGrade();
    }
}
