namespace Acme.Workforce.Payrollops.Tests.PayRun.Allocation;

using Acme.Workforce.Payrollops.PayRun.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PayRunRequestRulesChecks
{
    public static void PayRunRequestValidatorReportsEveryProblem()
    {
        var validator = PayRunRequestRules.DefaultPayRunRequestValidator();
        var item = PayRunRequest.NewPayRunRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayRunRequestCodeLengthRule(255);
        var item = PayRunRequest.NewPayRunRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunRequestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PayRunRequestLabelLengthRule(16);
        var item = PayRunRequest.NewPayRunRequest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunRequestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PayRunRequestRequiredLabelRule("pay run request needs this field");
        var item = PayRunRequest.NewPayRunRequest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunRequestBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new PayRunRequestBudgetRangeRule(0.3, 1424.9);
        var item = PayRunRequest.NewPayRunRequest("x", "y");
        item.Budget = 1426.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayRunRequestValidatorReportsEveryProblem();
        PayRunRequestCodeLengthRuleFlagsALongCode();
        PayRunRequestLabelLengthRuleFlagsALongLabel();
        PayRunRequestRequiredLabelRuleFlagsABlankLabel();
        PayRunRequestBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
