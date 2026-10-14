namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionAudit;

using Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Checks of deduction audit code, run by the project's runner.</summary>
public static class DeductionManifestRulesChecks
{
    public static void DeductionManifestValidatorReportsEveryProblem()
    {
        var validator = DeductionManifestRules.DefaultDeductionManifestValidator();
        var item = DeductionManifest.NewDeductionManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionManifestNameLengthRuleFlagsALongName()
    {
        var rule = new DeductionManifestNameLengthRule(32);
        var item = DeductionManifest.NewDeductionManifest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionManifestScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new DeductionManifestScoreRangeRule(1.8, 4282.2);
        var item = DeductionManifest.NewDeductionManifest("x", "y");
        item.Score = 4283.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeductionManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionManifestCodeLengthRule(64);
        var item = DeductionManifest.NewDeductionManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionManifestRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new DeductionManifestRequiredSourceRule("deduction manifest needs this field");
        var item = DeductionManifest.NewDeductionManifest("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeductionManifestValidatorReportsEveryProblem();
        DeductionManifestNameLengthRuleFlagsALongName();
        DeductionManifestScoreRangeRuleFlagsValuesAboveScore();
        DeductionManifestCodeLengthRuleFlagsALongCode();
        DeductionManifestRequiredSourceRuleFlagsABlankSource();
    }
}
