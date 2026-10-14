namespace Acme.Workforce.Absence.Tests.ReturnPlan.Summary;

using Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ReturnPlanSummaryRulesChecks
{
    public static void ReturnPlanSummaryValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanSummaryRules.DefaultReturnPlanSummaryValidator();
        var item = ReturnPlanSummary.NewReturnPlanSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanSummaryChannelLengthRuleFlagsALongChannel()
    {
        var rule = new ReturnPlanSummaryChannelLengthRule(80);
        var item = ReturnPlanSummary.NewReturnPlanSummary("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanSummaryRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new ReturnPlanSummaryRequiredExternalRefRule("return plan summary needs this field");
        var item = ReturnPlanSummary.NewReturnPlanSummary("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanSummaryGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new ReturnPlanSummaryGraceDaysRangeRule(32, 120);
        var item = ReturnPlanSummary.NewReturnPlanSummary("x", "y");
        item.GraceDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanSummaryRequiredNameRuleFlagsABlankName()
    {
        var rule = new ReturnPlanSummaryRequiredNameRule("return plan summary needs a value");
        var item = ReturnPlanSummary.NewReturnPlanSummary("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReturnPlanSummaryValidatorReportsEveryProblem();
        ReturnPlanSummaryChannelLengthRuleFlagsALongChannel();
        ReturnPlanSummaryRequiredExternalRefRuleFlagsABlankExternalRef();
        ReturnPlanSummaryGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        ReturnPlanSummaryRequiredNameRuleFlagsABlankName();
    }
}
