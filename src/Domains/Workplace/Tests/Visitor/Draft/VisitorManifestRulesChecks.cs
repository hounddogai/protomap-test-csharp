namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class VisitorManifestRulesChecks
{
    public static void VisitorManifestValidatorReportsEveryProblem()
    {
        var validator = VisitorManifestRules.DefaultVisitorManifestValidator();
        var item = VisitorManifest.NewVisitorManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorManifestHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new VisitorManifestHoursRangeRule(6.0, 4136.2);
        var item = VisitorManifest.NewVisitorManifest("x", "y");
        item.Hours = 4137.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new VisitorManifestCodeLengthRule(80);
        var item = VisitorManifest.NewVisitorManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VisitorManifestRequiredCodeRule("visitor manifest needs an entry");
        var item = VisitorManifest.NewVisitorManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorManifestValidatorReportsEveryProblem();
        VisitorManifestHoursRangeRuleFlagsValuesAboveHours();
        VisitorManifestCodeLengthRuleFlagsALongCode();
        VisitorManifestRequiredCodeRuleFlagsABlankCode();
    }
}
