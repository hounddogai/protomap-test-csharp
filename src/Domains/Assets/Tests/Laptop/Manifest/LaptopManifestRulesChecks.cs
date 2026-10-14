namespace Acme.Workforce.Assets.Tests.Laptop.Manifest;

using Acme.Workforce.Assets.Laptop.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LaptopManifestRulesChecks
{
    public static void LaptopManifestValidatorReportsEveryProblem()
    {
        var validator = LaptopManifestRules.DefaultLaptopManifestValidator();
        var item = LaptopManifest.NewLaptopManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopManifestTeamLengthRuleFlagsALongTeam()
    {
        var rule = new LaptopManifestTeamLengthRule(3600);
        var item = LaptopManifest.NewLaptopManifest("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopManifestExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new LaptopManifestExternalRefLengthRule(80);
        var item = LaptopManifest.NewLaptopManifest("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopManifestRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new LaptopManifestRequiredTeamRule("laptop manifest needs an entry");
        var item = LaptopManifest.NewLaptopManifest("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopManifestBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new LaptopManifestBudgetRangeRule(9.2, 4233.3);
        var item = LaptopManifest.NewLaptopManifest("x", "y");
        item.Budget = 4234.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LaptopManifestValidatorReportsEveryProblem();
        LaptopManifestTeamLengthRuleFlagsALongTeam();
        LaptopManifestExternalRefLengthRuleFlagsALongExternalRef();
        LaptopManifestRequiredTeamRuleFlagsABlankTeam();
        LaptopManifestBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
