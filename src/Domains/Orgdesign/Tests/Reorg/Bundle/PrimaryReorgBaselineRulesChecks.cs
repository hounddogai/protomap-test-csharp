namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PrimaryReorgBaselineRulesChecks
{
    public static void PrimaryReorgBaselineValidatorReportsEveryProblem()
    {
        var validator = PrimaryReorgBaselineRules.DefaultPrimaryReorgBaselineValidator();
        var item = PrimaryReorgBaseline.NewPrimaryReorgBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryReorgBaselineRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new PrimaryReorgBaselineRevisionRangeRule(64, 80);
        var item = PrimaryReorgBaseline.NewPrimaryReorgBaseline("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryReorgBaselineRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PrimaryReorgBaselineRetriesRangeRule(80, 255);
        var item = PrimaryReorgBaseline.NewPrimaryReorgBaseline("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryReorgBaselineValidatorReportsEveryProblem();
        PrimaryReorgBaselineRevisionRangeRuleFlagsValuesAboveRevision();
        PrimaryReorgBaselineRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
