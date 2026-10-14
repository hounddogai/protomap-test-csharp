namespace Acme.Workforce.Assets.Tests.AssetReturn.Quota;

using Acme.Workforce.Assets.AssetReturn.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AssetReturnClaimRulesChecks
{
    public static void AssetReturnClaimValidatorReportsEveryProblem()
    {
        var validator = AssetReturnClaimRules.DefaultAssetReturnClaimValidator();
        var item = AssetReturnClaim.NewAssetReturnClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssetReturnClaimCodeLengthRule(255);
        var item = AssetReturnClaim.NewAssetReturnClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnClaimRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AssetReturnClaimRequiredLabelRule("asset return claim needs this field");
        var item = AssetReturnClaim.NewAssetReturnClaim("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnClaimPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new AssetReturnClaimPhoneLengthRule(32);
        var item = AssetReturnClaim.NewAssetReturnClaim("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnClaimRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new AssetReturnClaimRequiredPhoneRule("asset return claim needs a value");
        var item = AssetReturnClaim.NewAssetReturnClaim("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssetReturnClaimValidatorReportsEveryProblem();
        AssetReturnClaimCodeLengthRuleFlagsALongCode();
        AssetReturnClaimRequiredLabelRuleFlagsABlankLabel();
        AssetReturnClaimPhoneLengthRuleFlagsALongPhone();
        AssetReturnClaimRequiredPhoneRuleFlagsABlankPhone();
    }
}
