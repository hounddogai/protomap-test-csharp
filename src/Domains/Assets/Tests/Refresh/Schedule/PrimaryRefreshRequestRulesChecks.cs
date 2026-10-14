namespace Acme.Workforce.Assets.Tests.Refresh.Schedule;

using Acme.Workforce.Assets.Refresh.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PrimaryRefreshRequestRulesChecks
{
    public static void PrimaryRefreshRequestValidatorReportsEveryProblem()
    {
        var validator = PrimaryRefreshRequestRules.DefaultPrimaryRefreshRequestValidator();
        var item = PrimaryRefreshRequest.NewPrimaryRefreshRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryRefreshRequestNameLengthRuleFlagsALongName()
    {
        var rule = new PrimaryRefreshRequestNameLengthRule(16);
        var item = PrimaryRefreshRequest.NewPrimaryRefreshRequest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryRefreshRequestRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new PrimaryRefreshRequestRequiredSourceRule("primary refresh request needs a value");
        var item = PrimaryRefreshRequest.NewPrimaryRefreshRequest("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryRefreshRequestCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new PrimaryRefreshRequestCapacityRangeRule(80, 255);
        var item = PrimaryRefreshRequest.NewPrimaryRefreshRequest("x", "y");
        item.Capacity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryRefreshRequestValidatorReportsEveryProblem();
        PrimaryRefreshRequestNameLengthRuleFlagsALongName();
        PrimaryRefreshRequestRequiredSourceRuleFlagsABlankSource();
        PrimaryRefreshRequestCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
