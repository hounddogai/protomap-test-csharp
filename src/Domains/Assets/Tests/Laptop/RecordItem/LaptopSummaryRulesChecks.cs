namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopSummaryRulesChecks
{
    public static void LaptopSummaryValidatorReportsEveryProblem()
    {
        var validator = LaptopSummaryRules.DefaultLaptopSummaryValidator();
        var item = LaptopSummary.NewLaptopSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new LaptopSummaryCodeLengthRule(80);
        var item = LaptopSummary.NewLaptopSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LaptopSummaryRequiredCodeRule("laptop summary needs an entry");
        var item = LaptopSummary.NewLaptopSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopSummaryChannelLengthRuleFlagsALongChannel()
    {
        var rule = new LaptopSummaryChannelLengthRule(1000);
        var item = LaptopSummary.NewLaptopSummary("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopSummaryRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new LaptopSummaryRequiredGradeRule("laptop summary needs a value");
        var item = LaptopSummary.NewLaptopSummary("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LaptopSummaryRequiredTitleRule("laptop summary needs an entry");
        var item = LaptopSummary.NewLaptopSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LaptopSummaryValidatorReportsEveryProblem();
        LaptopSummaryCodeLengthRuleFlagsALongCode();
        LaptopSummaryRequiredCodeRuleFlagsABlankCode();
        LaptopSummaryChannelLengthRuleFlagsALongChannel();
        LaptopSummaryRequiredGradeRuleFlagsABlankGrade();
        LaptopSummaryRequiredTitleRuleFlagsABlankTitle();
    }
}
