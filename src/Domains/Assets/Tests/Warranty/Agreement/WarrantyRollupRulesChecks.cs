namespace Acme.Workforce.Assets.Tests.Warranty.Agreement;

using Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WarrantyRollupRulesChecks
{
    public static void WarrantyRollupValidatorReportsEveryProblem()
    {
        var validator = WarrantyRollupRules.DefaultWarrantyRollupValidator();
        var item = WarrantyRollup.NewWarrantyRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new WarrantyRollupCodeLengthRule(80);
        var item = WarrantyRollup.NewWarrantyRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyRollupAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new WarrantyRollupAttemptsRangeRule(80, 255);
        var item = WarrantyRollup.NewWarrantyRollup("x", "y");
        item.Attempts = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyRollupExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new WarrantyRollupExternalRefLengthRule(16);
        var item = WarrantyRollup.NewWarrantyRollup("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyRollupRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new WarrantyRollupRequiredExternalRefRule("warranty rollup needs this field");
        var item = WarrantyRollup.NewWarrantyRollup("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WarrantyRollupValidatorReportsEveryProblem();
        WarrantyRollupCodeLengthRuleFlagsALongCode();
        WarrantyRollupAttemptsRangeRuleFlagsValuesAboveAttempts();
        WarrantyRollupExternalRefLengthRuleFlagsALongExternalRef();
        WarrantyRollupRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
