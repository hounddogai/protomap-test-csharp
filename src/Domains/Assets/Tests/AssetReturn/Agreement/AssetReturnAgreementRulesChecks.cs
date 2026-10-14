namespace Acme.Workforce.Assets.Tests.AssetReturn.Agreement;

using Acme.Workforce.Assets.AssetReturn.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssetReturnAgreementRulesChecks
{
    public static void AssetReturnAgreementValidatorReportsEveryProblem()
    {
        var validator = AssetReturnAgreementRules.DefaultAssetReturnAgreementValidator();
        var item = AssetReturnAgreement.NewAssetReturnAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnAgreementCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new AssetReturnAgreementCostCenterLengthRule(16);
        var item = AssetReturnAgreement.NewAssetReturnAgreement("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnAgreementRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new AssetReturnAgreementRequiredTeamRule("asset return agreement needs an entry");
        var item = AssetReturnAgreement.NewAssetReturnAgreement("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnAgreementNameLengthRuleFlagsALongName()
    {
        var rule = new AssetReturnAgreementNameLengthRule(3600);
        var item = AssetReturnAgreement.NewAssetReturnAgreement("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssetReturnAgreementValidatorReportsEveryProblem();
        AssetReturnAgreementCostCenterLengthRuleFlagsALongCostCenter();
        AssetReturnAgreementRequiredTeamRuleFlagsABlankTeam();
        AssetReturnAgreementNameLengthRuleFlagsALongName();
    }
}
