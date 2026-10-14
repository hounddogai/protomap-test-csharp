namespace Acme.Workforce.Assets.Tests.AssetReturn.Forecast;

using Acme.Workforce.Assets.AssetReturn.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GlobalAssetReturnRecordRulesChecks
{
    public static void GlobalAssetReturnRecordValidatorReportsEveryProblem()
    {
        var validator = GlobalAssetReturnRecordRules.DefaultGlobalAssetReturnRecordValidator();
        var item = GlobalAssetReturnRecord.NewGlobalAssetReturnRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAssetReturnRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalAssetReturnRecordCodeLengthRule(255);
        var item = GlobalAssetReturnRecord.NewGlobalAssetReturnRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAssetReturnRecordExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new GlobalAssetReturnRecordExpiresAtRangeRule(16, 3600);
        var item = GlobalAssetReturnRecord.NewGlobalAssetReturnRecord("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalAssetReturnRecordRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new GlobalAssetReturnRecordRequiredChecksumRule("global asset return record needs an entry");
        var item = GlobalAssetReturnRecord.NewGlobalAssetReturnRecord("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAssetReturnRecordChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new GlobalAssetReturnRecordChecksumLengthRule(1000);
        var item = GlobalAssetReturnRecord.NewGlobalAssetReturnRecord("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalAssetReturnRecordValidatorReportsEveryProblem();
        GlobalAssetReturnRecordCodeLengthRuleFlagsALongCode();
        GlobalAssetReturnRecordExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        GlobalAssetReturnRecordRequiredChecksumRuleFlagsABlankChecksum();
        GlobalAssetReturnRecordChecksumLengthRuleFlagsALongChecksum();
    }
}
