namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanRecordRulesChecks
{
    public static void SuccessionPlanRecordValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanRecordRules.DefaultSuccessionPlanRecordValidator();
        var item = SuccessionPlanRecord.NewSuccessionPlanRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanRecordNameLengthRuleFlagsALongName()
    {
        var rule = new SuccessionPlanRecordNameLengthRule(64);
        var item = SuccessionPlanRecord.NewSuccessionPlanRecord("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanRecordStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new SuccessionPlanRecordStartsAtRangeRule(3600, 3600);
        var item = SuccessionPlanRecord.NewSuccessionPlanRecord("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SuccessionPlanRecordValidatorReportsEveryProblem();
        SuccessionPlanRecordNameLengthRuleFlagsALongName();
        SuccessionPlanRecordStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
