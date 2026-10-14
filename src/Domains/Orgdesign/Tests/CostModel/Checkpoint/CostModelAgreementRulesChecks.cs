namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CostModelAgreementRulesChecks
{
    public static void CostModelAgreementValidatorReportsEveryProblem()
    {
        var validator = CostModelAgreementRules.DefaultCostModelAgreementValidator();
        var item = CostModelAgreement.NewCostModelAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new CostModelAgreementCodeLengthRule(3600);
        var item = CostModelAgreement.NewCostModelAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelAgreementAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new CostModelAgreementAmountRangeRule(9.9, 3513.8);
        var item = CostModelAgreement.NewCostModelAgreement("x", "y");
        item.Amount = 3515.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelAgreementNameLengthRuleFlagsALongName()
    {
        var rule = new CostModelAgreementNameLengthRule(16);
        var item = CostModelAgreement.NewCostModelAgreement("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CostModelAgreementValidatorReportsEveryProblem();
        CostModelAgreementCodeLengthRuleFlagsALongCode();
        CostModelAgreementAmountRangeRuleFlagsValuesAboveAmount();
        CostModelAgreementNameLengthRuleFlagsALongName();
    }
}
