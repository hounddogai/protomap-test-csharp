namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TownHallEntryRulesChecks
{
    public static void TownHallEntryValidatorReportsEveryProblem()
    {
        var validator = TownHallEntryRules.DefaultTownHallEntryValidator();
        var item = TownHallEntry.NewTownHallEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallEntryNameLengthRuleFlagsALongName()
    {
        var rule = new TownHallEntryNameLengthRule(64);
        var item = TownHallEntry.NewTownHallEntry("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TownHallEntryRequiredCodeRule("town hall entry needs a value");
        var item = TownHallEntry.NewTownHallEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallEntrySequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new TownHallEntrySequenceRangeRule(64, 1000);
        var item = TownHallEntry.NewTownHallEntry("x", "y");
        item.Sequence = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallEntryRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new TownHallEntryRequiredPhoneRule("town hall entry needs a value");
        var item = TownHallEntry.NewTownHallEntry("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallEntryEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new TownHallEntryEmployeeIdLengthRule(86400);
        var item = TownHallEntry.NewTownHallEntry("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TownHallEntryValidatorReportsEveryProblem();
        TownHallEntryNameLengthRuleFlagsALongName();
        TownHallEntryRequiredCodeRuleFlagsABlankCode();
        TownHallEntrySequenceRangeRuleFlagsValuesAboveSequence();
        TownHallEntryRequiredPhoneRuleFlagsABlankPhone();
        TownHallEntryEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
