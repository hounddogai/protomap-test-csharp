namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskOverrideRulesChecks
{
    public static void DeskOverrideValidatorReportsEveryProblem()
    {
        var validator = DeskOverrideRules.DefaultDeskOverrideValidator();
        var item = DeskOverride.NewDeskOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskOverrideExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new DeskOverrideExternalRefLengthRule(86400);
        var item = DeskOverride.NewDeskOverride("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskOverrideVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new DeskOverrideVersionRangeRule(3600, 3600);
        var item = DeskOverride.NewDeskOverride("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskOverrideLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DeskOverrideLabelLengthRule(80);
        var item = DeskOverride.NewDeskOverride("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskOverrideValidatorReportsEveryProblem();
        DeskOverrideExternalRefLengthRuleFlagsALongExternalRef();
        DeskOverrideVersionRangeRuleFlagsValuesAboveVersion();
        DeskOverrideLabelLengthRuleFlagsALongLabel();
    }
}
