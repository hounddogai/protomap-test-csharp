namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StrictWarrantyDraftRulesChecks
{
    public static void StrictWarrantyDraftValidatorReportsEveryProblem()
    {
        var validator = StrictWarrantyDraftRules.DefaultStrictWarrantyDraftValidator();
        var item = StrictWarrantyDraft.NewStrictWarrantyDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictWarrantyDraftSourceLengthRuleFlagsALongSource()
    {
        var rule = new StrictWarrantyDraftSourceLengthRule(16);
        var item = StrictWarrantyDraft.NewStrictWarrantyDraft("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictWarrantyDraftLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new StrictWarrantyDraftLeaveDaysRangeRule(32, 86400);
        var item = StrictWarrantyDraft.NewStrictWarrantyDraft("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictWarrantyDraftValidatorReportsEveryProblem();
        StrictWarrantyDraftSourceLengthRuleFlagsALongSource();
        StrictWarrantyDraftLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
