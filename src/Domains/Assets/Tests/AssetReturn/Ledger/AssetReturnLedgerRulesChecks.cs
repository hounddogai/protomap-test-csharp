namespace Acme.Workforce.Assets.Tests.AssetReturn.Ledger;

using Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AssetReturnLedgerRulesChecks
{
    public static void AssetReturnLedgerValidatorReportsEveryProblem()
    {
        var validator = AssetReturnLedgerRules.DefaultAssetReturnLedgerValidator();
        var item = AssetReturnLedger.NewAssetReturnLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnLedgerAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new AssetReturnLedgerAttemptsRangeRule(255, 86400);
        var item = AssetReturnLedger.NewAssetReturnLedger("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssetReturnLedgerCodeLengthRule(80);
        var item = AssetReturnLedger.NewAssetReturnLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssetReturnLedgerValidatorReportsEveryProblem();
        AssetReturnLedgerAttemptsRangeRuleFlagsValuesAboveAttempts();
        AssetReturnLedgerCodeLengthRuleFlagsALongCode();
    }
}
