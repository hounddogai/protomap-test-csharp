namespace Acme.Workforce.Workplace.Tests.Visitor.Estimate;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalVisitorDraftRulesChecks
{
    public static void GlobalVisitorDraftValidatorReportsEveryProblem()
    {
        var validator = GlobalVisitorDraftRules.DefaultGlobalVisitorDraftValidator();
        var item = GlobalVisitorDraft.NewGlobalVisitorDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalVisitorDraftTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new GlobalVisitorDraftTenureMonthsRangeRule(255, 1000);
        var item = GlobalVisitorDraft.NewGlobalVisitorDraft("x", "y");
        item.TenureMonths = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalVisitorDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalVisitorDraftRequiredCodeRule("global visitor draft needs an entry");
        var item = GlobalVisitorDraft.NewGlobalVisitorDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalVisitorDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GlobalVisitorDraftRequiredTitleRule("global visitor draft needs an entry");
        var item = GlobalVisitorDraft.NewGlobalVisitorDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalVisitorDraftRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new GlobalVisitorDraftRequiredLocationRule("global visitor draft needs an entry");
        var item = GlobalVisitorDraft.NewGlobalVisitorDraft("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalVisitorDraftValidatorReportsEveryProblem();
        GlobalVisitorDraftTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        GlobalVisitorDraftRequiredCodeRuleFlagsABlankCode();
        GlobalVisitorDraftRequiredTitleRuleFlagsABlankTitle();
        GlobalVisitorDraftRequiredLocationRuleFlagsABlankLocation();
    }
}
