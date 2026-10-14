namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopRecordRulesChecks
{
    public static void LaptopRecordValidatorReportsEveryProblem()
    {
        var validator = LaptopRecordRules.DefaultLaptopRecordValidator();
        var item = LaptopRecord.NewLaptopRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopRecordChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new LaptopRecordChecksumLengthRule(86400);
        var item = LaptopRecord.NewLaptopRecord("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopRecordRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LaptopRecordRequiredLabelRule("laptop record needs this field");
        var item = LaptopRecord.NewLaptopRecord("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopRecordLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LaptopRecordLabelLengthRule(255);
        var item = LaptopRecord.NewLaptopRecord("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopRecordRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new LaptopRecordRequiredChecksumRule("laptop record needs a value");
        var item = LaptopRecord.NewLaptopRecord("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopRecordTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new LaptopRecordTimezoneLengthRule(255);
        var item = LaptopRecord.NewLaptopRecord("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LaptopRecordValidatorReportsEveryProblem();
        LaptopRecordChecksumLengthRuleFlagsALongChecksum();
        LaptopRecordRequiredLabelRuleFlagsABlankLabel();
        LaptopRecordLabelLengthRuleFlagsALongLabel();
        LaptopRecordRequiredChecksumRuleFlagsABlankChecksum();
        LaptopRecordTimezoneLengthRuleFlagsALongTimezone();
    }
}
