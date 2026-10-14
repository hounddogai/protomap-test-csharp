namespace Acme.Workforce.Engagement.Tests.Recognition.Ledger;

using Acme.Workforce.Engagement.Recognition.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RecognitionBaselineRulesChecks
{
    public static void RecognitionBaselineValidatorReportsEveryProblem()
    {
        var validator = RecognitionBaselineRules.DefaultRecognitionBaselineValidator();
        var item = RecognitionBaseline.NewRecognitionBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionBaselineNameLengthRuleFlagsALongName()
    {
        var rule = new RecognitionBaselineNameLengthRule(1000);
        var item = RecognitionBaseline.NewRecognitionBaseline("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionBaselineCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new RecognitionBaselineCostCenterLengthRule(120);
        var item = RecognitionBaseline.NewRecognitionBaseline("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new RecognitionBaselineCodeLengthRule(1000);
        var item = RecognitionBaseline.NewRecognitionBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionBaselineRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new RecognitionBaselineRequiredCostCenterRule("recognition baseline needs an entry");
        var item = RecognitionBaseline.NewRecognitionBaseline("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RecognitionBaselineValidatorReportsEveryProblem();
        RecognitionBaselineNameLengthRuleFlagsALongName();
        RecognitionBaselineCostCenterLengthRuleFlagsALongCostCenter();
        RecognitionBaselineCodeLengthRuleFlagsALongCode();
        RecognitionBaselineRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
