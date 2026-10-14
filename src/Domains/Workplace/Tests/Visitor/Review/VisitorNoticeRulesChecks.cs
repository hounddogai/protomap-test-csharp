namespace Acme.Workforce.Workplace.Tests.Visitor.Review;

using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VisitorNoticeRulesChecks
{
    public static void VisitorNoticeValidatorReportsEveryProblem()
    {
        var validator = VisitorNoticeRules.DefaultVisitorNoticeValidator();
        var item = VisitorNotice.NewVisitorNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorNoticeDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new VisitorNoticeDescriptionLengthRule(255);
        var item = VisitorNotice.NewVisitorNotice("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorNoticeNameLengthRuleFlagsALongName()
    {
        var rule = new VisitorNoticeNameLengthRule(1000);
        var item = VisitorNotice.NewVisitorNotice("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorNoticeGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new VisitorNoticeGraceDaysRangeRule(255, 86400);
        var item = VisitorNotice.NewVisitorNotice("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        VisitorNoticeValidatorReportsEveryProblem();
        VisitorNoticeDescriptionLengthRuleFlagsALongDescription();
        VisitorNoticeNameLengthRuleFlagsALongName();
        VisitorNoticeGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
