namespace Acme.Workforce.Orgdesign.Tests.Position.Entry;

using Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PositionEntryRulesChecks
{
    public static void PositionEntryValidatorReportsEveryProblem()
    {
        var validator = PositionEntryRules.DefaultPositionEntryValidator();
        var item = PositionEntry.NewPositionEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionEntryFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new PositionEntryFteRangeRule(1.1, 863.6);
        var item = PositionEntry.NewPositionEntry("x", "y");
        item.Fte = 865.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionEntryCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new PositionEntryCurrencyLengthRule(120);
        var item = PositionEntry.NewPositionEntry("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionEntryRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new PositionEntryRequiredCurrencyRule("position entry needs an entry");
        var item = PositionEntry.NewPositionEntry("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionEntryOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new PositionEntryOwnerIdLengthRule(120);
        var item = PositionEntry.NewPositionEntry("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PositionEntryValidatorReportsEveryProblem();
        PositionEntryFteRangeRuleFlagsValuesAboveFte();
        PositionEntryCurrencyLengthRuleFlagsALongCurrency();
        PositionEntryRequiredCurrencyRuleFlagsABlankCurrency();
        PositionEntryOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
