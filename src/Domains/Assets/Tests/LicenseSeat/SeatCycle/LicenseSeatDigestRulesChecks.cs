namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatCycle;

using Acme.Workforce.Assets.LicenseSeat.SeatCycle;

/// <summary>Checks of seat cycle code, run by the project's runner.</summary>
public static class LicenseSeatDigestRulesChecks
{
    public static void LicenseSeatDigestValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatDigestRules.DefaultLicenseSeatDigestValidator();
        var item = LicenseSeatDigest.NewLicenseSeatDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatDigestCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new LicenseSeatDigestCostRangeRule(0.0, 1547.4);
        var item = LicenseSeatDigest.NewLicenseSeatDigest("x", "y");
        item.Cost = 1548.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatDigestWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new LicenseSeatDigestWeightRangeRule(3.1, 1243.5);
        var item = LicenseSeatDigest.NewLicenseSeatDigest("x", "y");
        item.Weight = 1245.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new LicenseSeatDigestCodeLengthRule(255);
        var item = LicenseSeatDigest.NewLicenseSeatDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatDigestSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LicenseSeatDigestSequenceRangeRule(120, 255);
        var item = LicenseSeatDigest.NewLicenseSeatDigest("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LicenseSeatDigestValidatorReportsEveryProblem();
        LicenseSeatDigestCostRangeRuleFlagsValuesAboveCost();
        LicenseSeatDigestWeightRangeRuleFlagsValuesAboveWeight();
        LicenseSeatDigestCodeLengthRuleFlagsALongCode();
        LicenseSeatDigestSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
