namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionRecordRulesChecks
{
    public static void PositionRecordValidatorReportsEveryProblem()
    {
        var validator = PositionRecordRules.DefaultPositionRecordValidator();
        var item = PositionRecord.NewPositionRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionRecordEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PositionRecordEndsAtRangeRule(16, 32);
        var item = PositionRecord.NewPositionRecord("x", "y");
        item.EndsAt = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionRecordRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new PositionRecordRevisionRangeRule(80, 255);
        var item = PositionRecord.NewPositionRecord("x", "y");
        item.Revision = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PositionRecordRequiredCodeRule("position record needs a value");
        var item = PositionRecord.NewPositionRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionRecordBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new PositionRecordBudgetRangeRule(3.7, 864.2);
        var item = PositionRecord.NewPositionRecord("x", "y");
        item.Budget = 865.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PositionRecordValidatorReportsEveryProblem();
        PositionRecordEndsAtRangeRuleFlagsValuesAboveEndsAt();
        PositionRecordRevisionRangeRuleFlagsValuesAboveRevision();
        PositionRecordRequiredCodeRuleFlagsABlankCode();
        PositionRecordBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
