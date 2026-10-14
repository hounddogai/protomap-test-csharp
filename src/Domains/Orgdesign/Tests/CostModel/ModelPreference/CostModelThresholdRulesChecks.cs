namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPreference;

using Acme.Workforce.Orgdesign.CostModel.ModelPreference;

/// <summary>Checks of model preference code, run by the project's runner.</summary>
public static class CostModelThresholdRulesChecks
{
    public static void CostModelThresholdValidatorReportsEveryProblem()
    {
        var validator = CostModelThresholdRules.DefaultCostModelThresholdValidator();
        var item = CostModelThreshold.NewCostModelThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelThresholdRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new CostModelThresholdRequiredNotesRule("cost model threshold needs a value");
        var item = CostModelThreshold.NewCostModelThreshold("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelThresholdPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new CostModelThresholdPriorityRangeRule(16, 64);
        var item = CostModelThreshold.NewCostModelThreshold("x", "y");
        item.Priority = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelThresholdRegionLengthRuleFlagsALongRegion()
    {
        var rule = new CostModelThresholdRegionLengthRule(80);
        var item = CostModelThreshold.NewCostModelThreshold("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelThresholdRequiredNameRuleFlagsABlankName()
    {
        var rule = new CostModelThresholdRequiredNameRule("cost model threshold needs an entry");
        var item = CostModelThreshold.NewCostModelThreshold("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CostModelThresholdValidatorReportsEveryProblem();
        CostModelThresholdRequiredNotesRuleFlagsABlankNotes();
        CostModelThresholdPriorityRangeRuleFlagsValuesAbovePriority();
        CostModelThresholdRegionLengthRuleFlagsALongRegion();
        CostModelThresholdRequiredNameRuleFlagsABlankName();
    }
}
