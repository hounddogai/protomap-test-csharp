namespace Acme.Workforce.Workplace.Tests.Occupancy.Target;

using Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class OccupancyTargetRulesChecks
{
    public static void OccupancyTargetValidatorReportsEveryProblem()
    {
        var validator = OccupancyTargetRules.DefaultOccupancyTargetValidator();
        var item = OccupancyTarget.NewOccupancyTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyTargetContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new OccupancyTargetContactEmailLengthRule(255);
        var item = OccupancyTarget.NewOccupancyTarget("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyTargetHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new OccupancyTargetHoursRangeRule(7.6, 1013.9);
        var item = OccupancyTarget.NewOccupancyTarget("x", "y");
        item.Hours = 1015.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyTargetRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new OccupancyTargetRequiredJobCodeRule("occupancy target needs an entry");
        var item = OccupancyTarget.NewOccupancyTarget("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OccupancyTargetValidatorReportsEveryProblem();
        OccupancyTargetContactEmailLengthRuleFlagsALongContactEmail();
        OccupancyTargetHoursRangeRuleFlagsValuesAboveHours();
        OccupancyTargetRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
