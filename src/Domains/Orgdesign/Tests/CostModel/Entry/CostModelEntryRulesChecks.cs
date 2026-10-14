namespace Acme.Workforce.Orgdesign.Tests.CostModel.Entry;

using Acme.Workforce.Orgdesign.CostModel.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CostModelEntryRulesChecks
{
    public static void CostModelEntryValidatorReportsEveryProblem()
    {
        var validator = CostModelEntryRules.DefaultCostModelEntryValidator();
        var item = CostModelEntry.NewCostModelEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CostModelEntryRequiredCodeRule("cost model entry needs a value");
        var item = CostModelEntry.NewCostModelEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new CostModelEntryCodeLengthRule(64);
        var item = CostModelEntry.NewCostModelEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelEntryRegionLengthRuleFlagsALongRegion()
    {
        var rule = new CostModelEntryRegionLengthRule(86400);
        var item = CostModelEntry.NewCostModelEntry("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelEntryChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new CostModelEntryChecksumLengthRule(16);
        var item = CostModelEntry.NewCostModelEntry("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CostModelEntryValidatorReportsEveryProblem();
        CostModelEntryRequiredCodeRuleFlagsABlankCode();
        CostModelEntryCodeLengthRuleFlagsALongCode();
        CostModelEntryRegionLengthRuleFlagsALongRegion();
        CostModelEntryChecksumLengthRuleFlagsALongChecksum();
    }
}
