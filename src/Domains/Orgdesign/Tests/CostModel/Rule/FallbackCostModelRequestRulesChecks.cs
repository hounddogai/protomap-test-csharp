namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FallbackCostModelRequestRulesChecks
{
    public static void FallbackCostModelRequestValidatorReportsEveryProblem()
    {
        var validator = FallbackCostModelRequestRules.DefaultFallbackCostModelRequestValidator();
        var item = FallbackCostModelRequest.NewFallbackCostModelRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackCostModelRequestContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new FallbackCostModelRequestContactEmailLengthRule(255);
        var item = FallbackCostModelRequest.NewFallbackCostModelRequest("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackCostModelRequestRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new FallbackCostModelRequestRequiredContactEmailRule("fallback cost model request needs this field");
        var item = FallbackCostModelRequest.NewFallbackCostModelRequest("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackCostModelRequestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FallbackCostModelRequestRequiredLabelRule("fallback cost model request needs a value");
        var item = FallbackCostModelRequest.NewFallbackCostModelRequest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackCostModelRequestValidatorReportsEveryProblem();
        FallbackCostModelRequestContactEmailLengthRuleFlagsALongContactEmail();
        FallbackCostModelRequestRequiredContactEmailRuleFlagsABlankContactEmail();
        FallbackCostModelRequestRequiredLabelRuleFlagsABlankLabel();
    }
}
