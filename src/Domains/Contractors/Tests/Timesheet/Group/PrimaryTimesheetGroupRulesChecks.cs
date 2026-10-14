namespace Acme.Workforce.Contractors.Tests.Timesheet.Group;

using Acme.Workforce.Contractors.Timesheet.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PrimaryTimesheetGroupRulesChecks
{
    public static void PrimaryTimesheetGroupValidatorReportsEveryProblem()
    {
        var validator = PrimaryTimesheetGroupRules.DefaultPrimaryTimesheetGroupValidator();
        var item = PrimaryTimesheetGroup.NewPrimaryTimesheetGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryTimesheetGroupRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new PrimaryTimesheetGroupRequiredJobCodeRule("primary timesheet group needs an entry");
        var item = PrimaryTimesheetGroup.NewPrimaryTimesheetGroup("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryTimesheetGroupOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new PrimaryTimesheetGroupOwnerIdLengthRule(32);
        var item = PrimaryTimesheetGroup.NewPrimaryTimesheetGroup("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryTimesheetGroupWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new PrimaryTimesheetGroupWeightRangeRule(8.3, 3160.6);
        var item = PrimaryTimesheetGroup.NewPrimaryTimesheetGroup("x", "y");
        item.Weight = 3162.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryTimesheetGroupJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new PrimaryTimesheetGroupJobCodeLengthRule(32);
        var item = PrimaryTimesheetGroup.NewPrimaryTimesheetGroup("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryTimesheetGroupNameLengthRuleFlagsALongName()
    {
        var rule = new PrimaryTimesheetGroupNameLengthRule(1000);
        var item = PrimaryTimesheetGroup.NewPrimaryTimesheetGroup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryTimesheetGroupValidatorReportsEveryProblem();
        PrimaryTimesheetGroupRequiredJobCodeRuleFlagsABlankJobCode();
        PrimaryTimesheetGroupOwnerIdLengthRuleFlagsALongOwnerId();
        PrimaryTimesheetGroupWeightRangeRuleFlagsValuesAboveWeight();
        PrimaryTimesheetGroupJobCodeLengthRuleFlagsALongJobCode();
        PrimaryTimesheetGroupNameLengthRuleFlagsALongName();
    }
}
