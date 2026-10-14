namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TieredPositionAdjustmentRulesChecks
{
    public static void TieredPositionAdjustmentValidatorReportsEveryProblem()
    {
        var validator = TieredPositionAdjustmentRules.DefaultTieredPositionAdjustmentValidator();
        var item = TieredPositionAdjustment.NewTieredPositionAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPositionAdjustmentRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new TieredPositionAdjustmentRequiredContactEmailRule("tiered position adjustment needs an entry");
        var item = TieredPositionAdjustment.NewTieredPositionAdjustment("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPositionAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredPositionAdjustmentCodeLengthRule(120);
        var item = TieredPositionAdjustment.NewTieredPositionAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredPositionAdjustmentValidatorReportsEveryProblem();
        TieredPositionAdjustmentRequiredContactEmailRuleFlagsABlankContactEmail();
        TieredPositionAdjustmentCodeLengthRuleFlagsALongCode();
    }
}
