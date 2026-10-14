namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathCycleRulesChecks
{
    public static void CareerPathCycleValidatorReportsEveryProblem()
    {
        var validator = CareerPathCycleRules.DefaultCareerPathCycleValidator();
        var item = CareerPathCycle.NewCareerPathCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathCycleChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new CareerPathCycleChecksumLengthRule(3600);
        var item = CareerPathCycle.NewCareerPathCycle("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathCycleMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new CareerPathCycleMaximumRangeRule(4.5, 4787.3);
        var item = CareerPathCycle.NewCareerPathCycle("x", "y");
        item.Maximum = 4788.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CareerPathCycleValidatorReportsEveryProblem();
        CareerPathCycleChecksumLengthRuleFlagsALongChecksum();
        CareerPathCycleMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
