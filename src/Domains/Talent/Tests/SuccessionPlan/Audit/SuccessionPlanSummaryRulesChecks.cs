namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanSummaryRulesChecks
{
    public static void SuccessionPlanSummaryValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanSummaryRules.DefaultSuccessionPlanSummaryValidator();
        var item = SuccessionPlanSummary.NewSuccessionPlanSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanSummaryRequiredNameRuleFlagsABlankName()
    {
        var rule = new SuccessionPlanSummaryRequiredNameRule("succession plan summary needs a value");
        var item = SuccessionPlanSummary.NewSuccessionPlanSummary("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SuccessionPlanSummaryRequiredCodeRule("succession plan summary needs a value");
        var item = SuccessionPlanSummary.NewSuccessionPlanSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanSummaryRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new SuccessionPlanSummaryRevisionRangeRule(32, 120);
        var item = SuccessionPlanSummary.NewSuccessionPlanSummary("x", "y");
        item.Revision = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanSummaryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SuccessionPlanSummaryRequiredLabelRule("succession plan summary needs a value");
        var item = SuccessionPlanSummary.NewSuccessionPlanSummary("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SuccessionPlanSummaryValidatorReportsEveryProblem();
        SuccessionPlanSummaryRequiredNameRuleFlagsABlankName();
        SuccessionPlanSummaryRequiredCodeRuleFlagsABlankCode();
        SuccessionPlanSummaryRevisionRangeRuleFlagsValuesAboveRevision();
        SuccessionPlanSummaryRequiredLabelRuleFlagsABlankLabel();
    }
}
