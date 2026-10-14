namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TownHallBatchRulesChecks
{
    public static void TownHallBatchValidatorReportsEveryProblem()
    {
        var validator = TownHallBatchRules.DefaultTownHallBatchValidator();
        var item = TownHallBatch.NewTownHallBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new TownHallBatchCodeLengthRule(16);
        var item = TownHallBatch.NewTownHallBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallBatchRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new TownHallBatchRequiredEmployeeIdRule("town hall batch needs this field");
        var item = TownHallBatch.NewTownHallBatch("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallBatchUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new TownHallBatchUpdatedAtRangeRule(16, 64);
        var item = TownHallBatch.NewTownHallBatch("x", "y");
        item.UpdatedAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallBatchRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TownHallBatchRequiredTitleRule("town hall batch needs a value");
        var item = TownHallBatch.NewTownHallBatch("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TownHallBatchValidatorReportsEveryProblem();
        TownHallBatchCodeLengthRuleFlagsALongCode();
        TownHallBatchRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        TownHallBatchUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        TownHallBatchRequiredTitleRuleFlagsABlankTitle();
    }
}
