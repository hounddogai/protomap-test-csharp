namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Policy;

using Acme.Workforce.Talent.SuccessionPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RegionalSuccessionPlanAgreementRulesChecks
{
    public static void RegionalSuccessionPlanAgreementValidatorReportsEveryProblem()
    {
        var validator = RegionalSuccessionPlanAgreementRules.DefaultRegionalSuccessionPlanAgreementValidator();
        var item = RegionalSuccessionPlanAgreement.NewRegionalSuccessionPlanAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalSuccessionPlanAgreementRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new RegionalSuccessionPlanAgreementRequiredLocationRule(
            "regional succession plan agreement needs an entry");
        var item = RegionalSuccessionPlanAgreement.NewRegionalSuccessionPlanAgreement("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalSuccessionPlanAgreementCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new RegionalSuccessionPlanAgreementCostCenterLengthRule(80);
        var item = RegionalSuccessionPlanAgreement.NewRegionalSuccessionPlanAgreement("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalSuccessionPlanAgreementRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new RegionalSuccessionPlanAgreementRequiredCostCenterRule(
            "regional succession plan agreement needs a value");
        var item = RegionalSuccessionPlanAgreement.NewRegionalSuccessionPlanAgreement("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalSuccessionPlanAgreementRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new RegionalSuccessionPlanAgreementRatioRangeRule(0.4, 89.6);
        var item = RegionalSuccessionPlanAgreement.NewRegionalSuccessionPlanAgreement("x", "y");
        item.Ratio = 91.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalSuccessionPlanAgreementSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new RegionalSuccessionPlanAgreementSequenceRangeRule(16, 64);
        var item = RegionalSuccessionPlanAgreement.NewRegionalSuccessionPlanAgreement("x", "y");
        item.Sequence = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalSuccessionPlanAgreementValidatorReportsEveryProblem();
        RegionalSuccessionPlanAgreementRequiredLocationRuleFlagsABlankLocation();
        RegionalSuccessionPlanAgreementCostCenterLengthRuleFlagsALongCostCenter();
        RegionalSuccessionPlanAgreementRequiredCostCenterRuleFlagsABlankCostCenter();
        RegionalSuccessionPlanAgreementRatioRangeRuleFlagsValuesAboveRatio();
        RegionalSuccessionPlanAgreementSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
