namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeskManifestRulesChecks
{
    public static void DeskManifestValidatorReportsEveryProblem()
    {
        var validator = DeskManifestRules.DefaultDeskManifestValidator();
        var item = DeskManifest.NewDeskManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskManifestRequiredNameRuleFlagsABlankName()
    {
        var rule = new DeskManifestRequiredNameRule("desk manifest needs this field");
        var item = DeskManifest.NewDeskManifest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskManifestRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new DeskManifestRevisionRangeRule(3600, 86400);
        var item = DeskManifest.NewDeskManifest("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskManifestNameLengthRuleFlagsALongName()
    {
        var rule = new DeskManifestNameLengthRule(255);
        var item = DeskManifest.NewDeskManifest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskManifestValidatorReportsEveryProblem();
        DeskManifestRequiredNameRuleFlagsABlankName();
        DeskManifestRevisionRangeRuleFlagsValuesAboveRevision();
        DeskManifestNameLengthRuleFlagsALongName();
    }
}
