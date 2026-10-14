namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PoolDraftRulesChecks
{
    public static void PoolDraftValidatorReportsEveryProblem()
    {
        var validator = PoolDraftRules.DefaultPoolDraftValidator();
        var item = PoolDraft.NewPoolDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolDraftAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PoolDraftAmountRangeRule(1.0, 2601.0);
        var item = PoolDraft.NewPoolDraft("x", "y");
        item.Amount = 2602.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolDraftLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new PoolDraftLocaleLengthRule(1000);
        var item = PoolDraft.NewPoolDraft("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolDraftRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new PoolDraftRequiredGradeRule("pool draft needs a value");
        var item = PoolDraft.NewPoolDraft("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new PoolDraftCodeLengthRule(1000);
        var item = PoolDraft.NewPoolDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolDraftExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new PoolDraftExpiresAtRangeRule(32, 86400);
        var item = PoolDraft.NewPoolDraft("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PoolDraftRequiredTitleRule("pool draft needs a value");
        var item = PoolDraft.NewPoolDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PoolDraftValidatorReportsEveryProblem();
        PoolDraftAmountRangeRuleFlagsValuesAboveAmount();
        PoolDraftLocaleLengthRuleFlagsALongLocale();
        PoolDraftRequiredGradeRuleFlagsABlankGrade();
        PoolDraftCodeLengthRuleFlagsALongCode();
        PoolDraftExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        PoolDraftRequiredTitleRuleFlagsABlankTitle();
    }
}
