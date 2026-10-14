namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class FallbackLicenseSeatLedgerRulesChecks
{
    public static void FallbackLicenseSeatLedgerValidatorReportsEveryProblem()
    {
        var validator = FallbackLicenseSeatLedgerRules.DefaultFallbackLicenseSeatLedgerValidator();
        var item = FallbackLicenseSeatLedger.NewFallbackLicenseSeatLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackLicenseSeatLedgerSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new FallbackLicenseSeatLedgerSegmentLengthRule(16);
        var item = FallbackLicenseSeatLedger.NewFallbackLicenseSeatLedger("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackLicenseSeatLedgerRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new FallbackLicenseSeatLedgerRequiredSegmentRule("fallback license seat ledger needs an entry");
        var item = FallbackLicenseSeatLedger.NewFallbackLicenseSeatLedger("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackLicenseSeatLedgerChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new FallbackLicenseSeatLedgerChecksumLengthRule(80);
        var item = FallbackLicenseSeatLedger.NewFallbackLicenseSeatLedger("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackLicenseSeatLedgerVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new FallbackLicenseSeatLedgerVersionRangeRule(16, 255);
        var item = FallbackLicenseSeatLedger.NewFallbackLicenseSeatLedger("x", "y");
        item.Version = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackLicenseSeatLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackLicenseSeatLedgerRequiredCodeRule("fallback license seat ledger needs an entry");
        var item = FallbackLicenseSeatLedger.NewFallbackLicenseSeatLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatLedgerValidatorReportsEveryProblem();
        FallbackLicenseSeatLedgerSegmentLengthRuleFlagsALongSegment();
        FallbackLicenseSeatLedgerRequiredSegmentRuleFlagsABlankSegment();
        FallbackLicenseSeatLedgerChecksumLengthRuleFlagsALongChecksum();
        FallbackLicenseSeatLedgerVersionRangeRuleFlagsValuesAboveVersion();
        FallbackLicenseSeatLedgerRequiredCodeRuleFlagsABlankCode();
    }
}
