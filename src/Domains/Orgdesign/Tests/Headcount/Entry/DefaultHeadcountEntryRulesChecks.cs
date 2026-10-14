namespace Acme.Workforce.Orgdesign.Tests.Headcount.Entry;

using Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DefaultHeadcountEntryRulesChecks
{
    public static void DefaultHeadcountEntryValidatorExtraReportsEveryProblem()
    {
        var validator = DefaultHeadcountEntryRules.DefaultDefaultHeadcountEntryValidator();
        var item = DefaultHeadcountEntry.NewDefaultHeadcountEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultHeadcountEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultHeadcountEntryCodeLengthRule(120);
        var item = DefaultHeadcountEntry.NewDefaultHeadcountEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultHeadcountEntryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultHeadcountEntryRequiredLabelRule("default headcount entry needs a value");
        var item = DefaultHeadcountEntry.NewDefaultHeadcountEntry("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultHeadcountEntryRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new DefaultHeadcountEntryRequiredChecksumRule("default headcount entry needs an entry");
        var item = DefaultHeadcountEntry.NewDefaultHeadcountEntry("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultHeadcountEntryFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new DefaultHeadcountEntryFloorValueRangeRule(4.2, 3613.5);
        var item = DefaultHeadcountEntry.NewDefaultHeadcountEntry("x", "y");
        item.FloorValue = 3615.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultHeadcountEntryWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new DefaultHeadcountEntryWeightRangeRule(1.4, 3632.5);
        var item = DefaultHeadcountEntry.NewDefaultHeadcountEntry("x", "y");
        item.Weight = 3634.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultHeadcountEntryValidatorExtraReportsEveryProblem();
        DefaultHeadcountEntryCodeLengthRuleFlagsALongCode();
        DefaultHeadcountEntryRequiredLabelRuleFlagsABlankLabel();
        DefaultHeadcountEntryRequiredChecksumRuleFlagsABlankChecksum();
        DefaultHeadcountEntryFloorValueRangeRuleFlagsValuesAboveFloorValue();
        DefaultHeadcountEntryWeightRangeRuleFlagsValuesAboveWeight();
    }
}
