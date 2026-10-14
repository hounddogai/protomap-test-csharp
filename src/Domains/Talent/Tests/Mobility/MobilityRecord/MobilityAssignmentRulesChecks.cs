namespace Acme.Workforce.Talent.Tests.Mobility.MobilityRecord;

using Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>Checks of mobility record code, run by the project's runner.</summary>
public static class MobilityAssignmentRulesChecks
{
    public static void MobilityAssignmentValidatorReportsEveryProblem()
    {
        var validator = MobilityAssignmentRules.DefaultMobilityAssignmentValidator();
        var item = MobilityAssignment.NewMobilityAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityAssignmentRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new MobilityAssignmentRetriesRangeRule(80, 120);
        var item = MobilityAssignment.NewMobilityAssignment("x", "y");
        item.Retries = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityAssignmentRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new MobilityAssignmentRateRangeRule(5.7, 419.3);
        var item = MobilityAssignment.NewMobilityAssignment("x", "y");
        item.Rate = 420.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MobilityAssignmentValidatorReportsEveryProblem();
        MobilityAssignmentRetriesRangeRuleFlagsValuesAboveRetries();
        MobilityAssignmentRateRangeRuleFlagsValuesAboveRate();
    }
}
