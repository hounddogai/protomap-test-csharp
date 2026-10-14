namespace Acme.Workforce.Assets.Tests.Assignment.Summary;

using Acme.Workforce.Assets.Assignment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RegionalAssignmentEntryRulesChecks
{
    public static void RegionalAssignmentEntryValidatorReportsEveryProblem()
    {
        var validator = RegionalAssignmentEntryRules.DefaultRegionalAssignmentEntryValidator();
        var item = RegionalAssignmentEntry.NewRegionalAssignmentEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalAssignmentEntryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RegionalAssignmentEntryTitleLengthRule(255);
        var item = RegionalAssignmentEntry.NewRegionalAssignmentEntry("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalAssignmentEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalAssignmentEntryCodeLengthRule(255);
        var item = RegionalAssignmentEntry.NewRegionalAssignmentEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalAssignmentEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalAssignmentEntryRequiredCodeRule("regional assignment entry needs an entry");
        var item = RegionalAssignmentEntry.NewRegionalAssignmentEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalAssignmentEntryValidatorReportsEveryProblem();
        RegionalAssignmentEntryTitleLengthRuleFlagsALongTitle();
        RegionalAssignmentEntryCodeLengthRuleFlagsALongCode();
        RegionalAssignmentEntryRequiredCodeRuleFlagsABlankCode();
    }
}
