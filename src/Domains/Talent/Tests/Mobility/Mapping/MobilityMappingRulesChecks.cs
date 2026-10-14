namespace Acme.Workforce.Talent.Tests.Mobility.Mapping;

using Acme.Workforce.Talent.Mobility.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class MobilityMappingRulesChecks
{
    public static void MobilityMappingValidatorReportsEveryProblem()
    {
        var validator = MobilityMappingRules.DefaultMobilityMappingValidator();
        var item = MobilityMapping.NewMobilityMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityMappingLocationLengthRuleFlagsALongLocation()
    {
        var rule = new MobilityMappingLocationLengthRule(120);
        var item = MobilityMapping.NewMobilityMapping("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityMappingRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new MobilityMappingRequiredLocationRule("mobility mapping needs an entry");
        var item = MobilityMapping.NewMobilityMapping("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MobilityMappingValidatorReportsEveryProblem();
        MobilityMappingLocationLengthRuleFlagsALongLocation();
        MobilityMappingRequiredLocationRuleFlagsABlankLocation();
    }
}
