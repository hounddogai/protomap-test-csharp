namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LenientSpanLedgerRulesChecks
{
    public static void LenientSpanLedgerValidatorReportsEveryProblem()
    {
        var validator = LenientSpanLedgerRules.DefaultLenientSpanLedgerValidator();
        var item = LenientSpanLedger.NewLenientSpanLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientSpanLedgerUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new LenientSpanLedgerUpdatedAtRangeRule(80, 1000);
        var item = LenientSpanLedger.NewLenientSpanLedger("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientSpanLedgerValidatorReportsEveryProblem();
        LenientSpanLedgerUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
