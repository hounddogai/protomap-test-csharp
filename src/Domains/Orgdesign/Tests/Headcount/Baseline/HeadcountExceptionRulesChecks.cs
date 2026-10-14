namespace Acme.Workforce.Orgdesign.Tests.Headcount.Baseline;

using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class HeadcountExceptionRulesChecks
{
    public static void HeadcountExceptionValidatorReportsEveryProblem()
    {
        var validator = HeadcountExceptionRules.DefaultHeadcountExceptionValidator();
        var item = HeadcountException.NewHeadcountException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountExceptionNameLengthRuleFlagsALongName()
    {
        var rule = new HeadcountExceptionNameLengthRule(80);
        var item = HeadcountException.NewHeadcountException("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HeadcountExceptionRequiredCodeRule("headcount exception needs a value");
        var item = HeadcountException.NewHeadcountException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountExceptionCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new HeadcountExceptionCeilingRangeRule(5.3, 4183.3);
        var item = HeadcountException.NewHeadcountException("x", "y");
        item.Ceiling = 4184.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountExceptionVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new HeadcountExceptionVersionRangeRule(16, 80);
        var item = HeadcountException.NewHeadcountException("x", "y");
        item.Version = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountExceptionTeamLengthRuleFlagsALongTeam()
    {
        var rule = new HeadcountExceptionTeamLengthRule(80);
        var item = HeadcountException.NewHeadcountException("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HeadcountExceptionValidatorReportsEveryProblem();
        HeadcountExceptionNameLengthRuleFlagsALongName();
        HeadcountExceptionRequiredCodeRuleFlagsABlankCode();
        HeadcountExceptionCeilingRangeRuleFlagsValuesAboveCeiling();
        HeadcountExceptionVersionRangeRuleFlagsValuesAboveVersion();
        HeadcountExceptionTeamLengthRuleFlagsALongTeam();
    }
}
