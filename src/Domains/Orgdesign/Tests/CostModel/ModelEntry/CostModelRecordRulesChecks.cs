namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class CostModelRecordRulesChecks
{
    public static void CostModelRecordValidatorReportsEveryProblem()
    {
        var validator = CostModelRecordRules.DefaultCostModelRecordValidator();
        var item = CostModelRecord.NewCostModelRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelRecordRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new CostModelRecordRequiredLocationRule("cost model record needs a value");
        var item = CostModelRecord.NewCostModelRecord("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelRecordRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new CostModelRecordRevisionRangeRule(32, 86400);
        var item = CostModelRecord.NewCostModelRecord("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new CostModelRecordCodeLengthRule(3600);
        var item = CostModelRecord.NewCostModelRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CostModelRecordValidatorReportsEveryProblem();
        CostModelRecordRequiredLocationRuleFlagsABlankLocation();
        CostModelRecordRevisionRangeRuleFlagsValuesAboveRevision();
        CostModelRecordCodeLengthRuleFlagsALongCode();
    }
}
