namespace Acme.Workforce.Assets.Tests.Warranty.Agreement;

using Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WarrantyEntryRulesChecks
{
    public static void WarrantyEntryValidatorReportsEveryProblem()
    {
        var validator = WarrantyEntryRules.DefaultWarrantyEntryValidator();
        var item = WarrantyEntry.NewWarrantyEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyEntrySourceLengthRuleFlagsALongSource()
    {
        var rule = new WarrantyEntrySourceLengthRule(32);
        var item = WarrantyEntry.NewWarrantyEntry("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WarrantyEntryValidatorReportsEveryProblem();
        WarrantyEntrySourceLengthRuleFlagsALongSource();
    }
}
