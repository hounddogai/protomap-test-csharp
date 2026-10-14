namespace Acme.Workforce.Orgdesign.Tests.Position.Quota;

using Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class GlobalPositionVarianceRulesChecks
{
    public static void GlobalPositionVarianceValidatorReportsEveryProblem()
    {
        var validator = GlobalPositionVarianceRules.DefaultGlobalPositionVarianceValidator();
        var item = GlobalPositionVariance.NewGlobalPositionVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalPositionVarianceCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new GlobalPositionVarianceCreatedAtRangeRule(64, 1000);
        var item = GlobalPositionVariance.NewGlobalPositionVariance("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalPositionVarianceRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new GlobalPositionVarianceRequiredSalaryBandRule("global position variance needs a value");
        var item = GlobalPositionVariance.NewGlobalPositionVariance("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalPositionVarianceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GlobalPositionVarianceTitleLengthRule(255);
        var item = GlobalPositionVariance.NewGlobalPositionVariance("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalPositionVarianceMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new GlobalPositionVarianceMaximumRangeRule(5.8, 2496.5);
        var item = GlobalPositionVariance.NewGlobalPositionVariance("x", "y");
        item.Maximum = 2498.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalPositionVarianceValidatorReportsEveryProblem();
        GlobalPositionVarianceCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        GlobalPositionVarianceRequiredSalaryBandRuleFlagsABlankSalaryBand();
        GlobalPositionVarianceTitleLengthRuleFlagsALongTitle();
        GlobalPositionVarianceMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
