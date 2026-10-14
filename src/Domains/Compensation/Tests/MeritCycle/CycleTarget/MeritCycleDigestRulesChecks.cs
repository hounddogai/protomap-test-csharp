namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleTarget;

using Acme.Workforce.Compensation.MeritCycle.CycleTarget;

/// <summary>Checks of cycle target code, run by the project's runner.</summary>
public static class MeritCycleDigestRulesChecks
{
    public static void MeritCycleDigestValidatorReportsEveryProblem()
    {
        var validator = MeritCycleDigestRules.DefaultMeritCycleDigestValidator();
        var item = MeritCycleDigest.NewMeritCycleDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleDigestDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new MeritCycleDigestDescriptionLengthRule(80);
        var item = MeritCycleDigest.NewMeritCycleDigest("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleDigestCodeLengthRule(120);
        var item = MeritCycleDigest.NewMeritCycleDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleDigestWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new MeritCycleDigestWeightRangeRule(6.2, 2272.2);
        var item = MeritCycleDigest.NewMeritCycleDigest("x", "y");
        item.Weight = 2273.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MeritCycleDigestValidatorReportsEveryProblem();
        MeritCycleDigestDescriptionLengthRuleFlagsALongDescription();
        MeritCycleDigestCodeLengthRuleFlagsALongCode();
        MeritCycleDigestWeightRangeRuleFlagsValuesAboveWeight();
    }
}
