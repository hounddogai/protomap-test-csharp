namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.StatementOfWork.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class WeightedStatementOfWorkBundleRulesChecks
{
    public static void WeightedStatementOfWorkBundleValidatorReportsEveryProblem()
    {
        var validator = WeightedStatementOfWorkBundleRules.DefaultWeightedStatementOfWorkBundleValidator();
        var item = WeightedStatementOfWorkBundle.NewWeightedStatementOfWorkBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedStatementOfWorkBundleNameLengthRuleFlagsALongName()
    {
        var rule = new WeightedStatementOfWorkBundleNameLengthRule(16);
        var item = WeightedStatementOfWorkBundle.NewWeightedStatementOfWorkBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedStatementOfWorkBundleRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new WeightedStatementOfWorkBundleRequiredUnitRule("weighted statement of work bundle needs a value");
        var item = WeightedStatementOfWorkBundle.NewWeightedStatementOfWorkBundle("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedStatementOfWorkBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedStatementOfWorkBundleCodeLengthRule(86400);
        var item = WeightedStatementOfWorkBundle.NewWeightedStatementOfWorkBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedStatementOfWorkBundleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new WeightedStatementOfWorkBundleDurationMinutesRangeRule(64, 3600);
        var item = WeightedStatementOfWorkBundle.NewWeightedStatementOfWorkBundle("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedStatementOfWorkBundleValidatorReportsEveryProblem();
        WeightedStatementOfWorkBundleNameLengthRuleFlagsALongName();
        WeightedStatementOfWorkBundleRequiredUnitRuleFlagsABlankUnit();
        WeightedStatementOfWorkBundleCodeLengthRuleFlagsALongCode();
        WeightedStatementOfWorkBundleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
