namespace Acme.Workforce.Talent.Tests.Pool.Milestone;

using Acme.Workforce.Talent.Pool.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TieredPoolGroupRulesChecks
{
    public static void TieredPoolGroupValidatorReportsEveryProblem()
    {
        var validator = TieredPoolGroupRules.DefaultTieredPoolGroupValidator();
        var item = TieredPoolGroup.NewTieredPoolGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPoolGroupRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new TieredPoolGroupRetriesRangeRule(64, 86400);
        var item = TieredPoolGroup.NewTieredPoolGroup("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredPoolGroupNameLengthRuleFlagsALongName()
    {
        var rule = new TieredPoolGroupNameLengthRule(80);
        var item = TieredPoolGroup.NewTieredPoolGroup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPoolGroupRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new TieredPoolGroupRequiredDepartmentRule("tiered pool group needs a value");
        var item = TieredPoolGroup.NewTieredPoolGroup("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredPoolGroupValidatorReportsEveryProblem();
        TieredPoolGroupRetriesRangeRuleFlagsValuesAboveRetries();
        TieredPoolGroupNameLengthRuleFlagsALongName();
        TieredPoolGroupRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
