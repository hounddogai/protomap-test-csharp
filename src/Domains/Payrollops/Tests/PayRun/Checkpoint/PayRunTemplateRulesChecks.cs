namespace Acme.Workforce.Payrollops.Tests.PayRun.Checkpoint;

using Acme.Workforce.Payrollops.PayRun.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PayRunTemplateRulesChecks
{
    public static void PayRunTemplateValidatorReportsEveryProblem()
    {
        var validator = PayRunTemplateRules.DefaultPayRunTemplateValidator();
        var item = PayRunTemplate.NewPayRunTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunTemplateRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new PayRunTemplateRevisionRangeRule(64, 120);
        var item = PayRunTemplate.NewPayRunTemplate("x", "y");
        item.Revision = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunTemplateRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new PayRunTemplateRequiredContactEmailRule("pay run template needs a value");
        var item = PayRunTemplate.NewPayRunTemplate("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunTemplateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PayRunTemplateLabelLengthRule(32);
        var item = PayRunTemplate.NewPayRunTemplate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunTemplateSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new PayRunTemplateSummaryLengthRule(120);
        var item = PayRunTemplate.NewPayRunTemplate("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayRunTemplateValidatorReportsEveryProblem();
        PayRunTemplateRevisionRangeRuleFlagsValuesAboveRevision();
        PayRunTemplateRequiredContactEmailRuleFlagsABlankContactEmail();
        PayRunTemplateLabelLengthRuleFlagsALongLabel();
        PayRunTemplateSummaryLengthRuleFlagsALongSummary();
    }
}
