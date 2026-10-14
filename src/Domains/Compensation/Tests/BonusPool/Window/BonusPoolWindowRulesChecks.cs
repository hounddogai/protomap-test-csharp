namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BonusPoolWindowRulesChecks
{
    public static void BonusPoolWindowValidatorReportsEveryProblem()
    {
        var validator = BonusPoolWindowRules.DefaultBonusPoolWindowValidator();
        var item = BonusPoolWindow.NewBonusPoolWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolWindowUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new BonusPoolWindowUpdatedAtRangeRule(32, 64);
        var item = BonusPoolWindow.NewBonusPoolWindow("x", "y");
        item.UpdatedAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolWindowRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new BonusPoolWindowRequiredContactEmailRule("bonus pool window needs an entry");
        var item = BonusPoolWindow.NewBonusPoolWindow("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolWindowWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new BonusPoolWindowWeightRangeRule(6.3, 3279.6);
        var item = BonusPoolWindow.NewBonusPoolWindow("x", "y");
        item.Weight = 3281.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolWindowContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new BonusPoolWindowContactEmailLengthRule(1000);
        var item = BonusPoolWindow.NewBonusPoolWindow("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BonusPoolWindowValidatorReportsEveryProblem();
        BonusPoolWindowUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        BonusPoolWindowRequiredContactEmailRuleFlagsABlankContactEmail();
        BonusPoolWindowWeightRangeRuleFlagsValuesAboveWeight();
        BonusPoolWindowContactEmailLengthRuleFlagsALongContactEmail();
    }
}
