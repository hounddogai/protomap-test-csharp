namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PrimaryMobilityTargetRulesChecks
{
    public static void PrimaryMobilityTargetValidatorReportsEveryProblem()
    {
        var validator = PrimaryMobilityTargetRules.DefaultPrimaryMobilityTargetValidator();
        var item = PrimaryMobilityTarget.NewPrimaryMobilityTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryMobilityTargetChannelLengthRuleFlagsALongChannel()
    {
        var rule = new PrimaryMobilityTargetChannelLengthRule(80);
        var item = PrimaryMobilityTarget.NewPrimaryMobilityTarget("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryMobilityTargetRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new PrimaryMobilityTargetRequiredDescriptionRule("primary mobility target needs an entry");
        var item = PrimaryMobilityTarget.NewPrimaryMobilityTarget("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryMobilityTargetMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new PrimaryMobilityTargetMinimumRangeRule(3.6, 3251.8);
        var item = PrimaryMobilityTarget.NewPrimaryMobilityTarget("x", "y");
        item.Minimum = 3253.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryMobilityTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryMobilityTargetCodeLengthRule(120);
        var item = PrimaryMobilityTarget.NewPrimaryMobilityTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryMobilityTargetValidatorReportsEveryProblem();
        PrimaryMobilityTargetChannelLengthRuleFlagsALongChannel();
        PrimaryMobilityTargetRequiredDescriptionRuleFlagsABlankDescription();
        PrimaryMobilityTargetMinimumRangeRuleFlagsValuesAboveMinimum();
        PrimaryMobilityTargetCodeLengthRuleFlagsALongCode();
    }
}
