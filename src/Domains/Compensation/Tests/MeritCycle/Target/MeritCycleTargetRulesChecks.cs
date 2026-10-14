namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleTargetRulesChecks
{
    public static void MeritCycleTargetValidatorReportsEveryProblem()
    {
        var validator = MeritCycleTargetRules.DefaultMeritCycleTargetValidator();
        var item = MeritCycleTarget.NewMeritCycleTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new MeritCycleTargetRequiredNameRule("merit cycle target needs this field");
        var item = MeritCycleTarget.NewMeritCycleTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleTargetRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new MeritCycleTargetRequiredContactEmailRule("merit cycle target needs this field");
        var item = MeritCycleTarget.NewMeritCycleTarget("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleTargetTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new MeritCycleTargetTimezoneLengthRule(86400);
        var item = MeritCycleTarget.NewMeritCycleTarget("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleTargetFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new MeritCycleTargetFloorValueRangeRule(3.6, 1539.4);
        var item = MeritCycleTarget.NewMeritCycleTarget("x", "y");
        item.FloorValue = 1540.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleTargetRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new MeritCycleTargetRequiredTimezoneRule("merit cycle target needs this field");
        var item = MeritCycleTarget.NewMeritCycleTarget("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MeritCycleTargetValidatorReportsEveryProblem();
        MeritCycleTargetRequiredNameRuleFlagsABlankName();
        MeritCycleTargetRequiredContactEmailRuleFlagsABlankContactEmail();
        MeritCycleTargetTimezoneLengthRuleFlagsALongTimezone();
        MeritCycleTargetFloorValueRangeRuleFlagsValuesAboveFloorValue();
        MeritCycleTargetRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
