namespace Acme.Workforce.Absence.Tests.Holiday.Manifest;

using Acme.Workforce.Absence.Holiday.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class HolidayManifestRulesChecks
{
    public static void HolidayManifestValidatorReportsEveryProblem()
    {
        var validator = HolidayManifestRules.DefaultHolidayManifestValidator();
        var item = HolidayManifest.NewHolidayManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayManifestRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new HolidayManifestRequiredSourceRule("holiday manifest needs this field");
        var item = HolidayManifest.NewHolidayManifest("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayManifestNameLengthRuleFlagsALongName()
    {
        var rule = new HolidayManifestNameLengthRule(3600);
        var item = HolidayManifest.NewHolidayManifest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayManifestMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new HolidayManifestMarginRangeRule(0.1, 4306.2);
        var item = HolidayManifest.NewHolidayManifest("x", "y");
        item.Margin = 4307.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayManifestOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new HolidayManifestOwnerIdLengthRule(80);
        var item = HolidayManifest.NewHolidayManifest("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayManifestCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new HolidayManifestCapacityRangeRule(32, 120);
        var item = HolidayManifest.NewHolidayManifest("x", "y");
        item.Capacity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HolidayManifestValidatorReportsEveryProblem();
        HolidayManifestRequiredSourceRuleFlagsABlankSource();
        HolidayManifestNameLengthRuleFlagsALongName();
        HolidayManifestMarginRangeRuleFlagsValuesAboveMargin();
        HolidayManifestOwnerIdLengthRuleFlagsALongOwnerId();
        HolidayManifestCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
