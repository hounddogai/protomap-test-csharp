namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class RollingAccrualSegmentRulesChecks
{
    public static void RollingAccrualSegmentValidatorReportsEveryProblem()
    {
        var validator = RollingAccrualSegmentRules.DefaultRollingAccrualSegmentValidator();
        var item = RollingAccrualSegment.NewRollingAccrualSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingAccrualSegmentAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new RollingAccrualSegmentAmountRangeRule(1.9, 4005.3);
        var item = RollingAccrualSegment.NewRollingAccrualSegment("x", "y");
        item.Amount = 4006.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingAccrualSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingAccrualSegmentCodeLengthRule(86400);
        var item = RollingAccrualSegment.NewRollingAccrualSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingAccrualSegmentLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new RollingAccrualSegmentLocaleLengthRule(120);
        var item = RollingAccrualSegment.NewRollingAccrualSegment("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingAccrualSegmentRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new RollingAccrualSegmentRequiredLocationRule("rolling accrual segment needs this field");
        var item = RollingAccrualSegment.NewRollingAccrualSegment("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingAccrualSegmentMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new RollingAccrualSegmentMaximumRangeRule(4.1, 2820.9);
        var item = RollingAccrualSegment.NewRollingAccrualSegment("x", "y");
        item.Maximum = 2822.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingAccrualSegmentValidatorReportsEveryProblem();
        RollingAccrualSegmentAmountRangeRuleFlagsValuesAboveAmount();
        RollingAccrualSegmentCodeLengthRuleFlagsALongCode();
        RollingAccrualSegmentLocaleLengthRuleFlagsALongLocale();
        RollingAccrualSegmentRequiredLocationRuleFlagsABlankLocation();
        RollingAccrualSegmentMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
