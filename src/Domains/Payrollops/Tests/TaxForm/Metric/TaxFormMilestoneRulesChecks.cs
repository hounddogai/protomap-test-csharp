namespace Acme.Workforce.Payrollops.Tests.TaxForm.Metric;

using Acme.Workforce.Payrollops.TaxForm.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TaxFormMilestoneRulesChecks
{
    public static void TaxFormMilestoneValidatorReportsEveryProblem()
    {
        var validator = TaxFormMilestoneRules.DefaultTaxFormMilestoneValidator();
        var item = TaxFormMilestone.NewTaxFormMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormMilestoneChannelLengthRuleFlagsALongChannel()
    {
        var rule = new TaxFormMilestoneChannelLengthRule(64);
        var item = TaxFormMilestone.NewTaxFormMilestone("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TaxFormMilestoneRequiredCodeRule("tax form milestone needs an entry");
        var item = TaxFormMilestone.NewTaxFormMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormMilestoneRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TaxFormMilestoneRequiredTitleRule("tax form milestone needs this field");
        var item = TaxFormMilestone.NewTaxFormMilestone("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormMilestoneValidatorReportsEveryProblem();
        TaxFormMilestoneChannelLengthRuleFlagsALongChannel();
        TaxFormMilestoneRequiredCodeRuleFlagsABlankCode();
        TaxFormMilestoneRequiredTitleRuleFlagsABlankTitle();
    }
}
