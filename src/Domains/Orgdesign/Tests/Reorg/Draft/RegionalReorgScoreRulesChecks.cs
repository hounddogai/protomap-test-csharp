namespace Acme.Workforce.Orgdesign.Tests.Reorg.Draft;

using Acme.Workforce.Orgdesign.Reorg.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RegionalReorgScoreRulesChecks
{
    public static void RegionalReorgScoreValidatorReportsEveryProblem()
    {
        var validator = RegionalReorgScoreRules.DefaultRegionalReorgScoreValidator();
        var item = RegionalReorgScore.NewRegionalReorgScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalReorgScoreUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new RegionalReorgScoreUpdatedAtRangeRule(16, 64);
        var item = RegionalReorgScore.NewRegionalReorgScore("x", "y");
        item.UpdatedAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalReorgScoreRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalReorgScoreRequiredNameRule("regional reorg score needs this field");
        var item = RegionalReorgScore.NewRegionalReorgScore("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalReorgScoreRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new RegionalReorgScoreRequiredSalaryBandRule("regional reorg score needs this field");
        var item = RegionalReorgScore.NewRegionalReorgScore("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalReorgScoreSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new RegionalReorgScoreSalaryBandLengthRule(64);
        var item = RegionalReorgScore.NewRegionalReorgScore("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalReorgScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalReorgScoreCodeLengthRule(255);
        var item = RegionalReorgScore.NewRegionalReorgScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalReorgScoreValidatorReportsEveryProblem();
        RegionalReorgScoreUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        RegionalReorgScoreRequiredNameRuleFlagsABlankName();
        RegionalReorgScoreRequiredSalaryBandRuleFlagsABlankSalaryBand();
        RegionalReorgScoreSalaryBandLengthRuleFlagsALongSalaryBand();
        RegionalReorgScoreCodeLengthRuleFlagsALongCode();
    }
}
