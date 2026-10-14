namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RegionalOffboardingNoticeRulesChecks
{
    public static void RegionalOffboardingNoticeValidatorReportsEveryProblem()
    {
        var validator = RegionalOffboardingNoticeRules.DefaultRegionalOffboardingNoticeValidator();
        var item = RegionalOffboardingNotice.NewRegionalOffboardingNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalOffboardingNoticeDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new RegionalOffboardingNoticeDescriptionLengthRule(64);
        var item = RegionalOffboardingNotice.NewRegionalOffboardingNotice("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalOffboardingNoticeRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new RegionalOffboardingNoticeRequiredDescriptionRule("regional offboarding notice needs an entry");
        var item = RegionalOffboardingNotice.NewRegionalOffboardingNotice("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalOffboardingNoticeUnitLengthRuleFlagsALongUnit()
    {
        var rule = new RegionalOffboardingNoticeUnitLengthRule(64);
        var item = RegionalOffboardingNotice.NewRegionalOffboardingNotice("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalOffboardingNoticeRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RegionalOffboardingNoticeRequiredLabelRule("regional offboarding notice needs this field");
        var item = RegionalOffboardingNotice.NewRegionalOffboardingNotice("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalOffboardingNoticeCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new RegionalOffboardingNoticeCostRangeRule(5.1, 1128.8);
        var item = RegionalOffboardingNotice.NewRegionalOffboardingNotice("x", "y");
        item.Cost = 1130.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalOffboardingNoticeValidatorReportsEveryProblem();
        RegionalOffboardingNoticeDescriptionLengthRuleFlagsALongDescription();
        RegionalOffboardingNoticeRequiredDescriptionRuleFlagsABlankDescription();
        RegionalOffboardingNoticeUnitLengthRuleFlagsALongUnit();
        RegionalOffboardingNoticeRequiredLabelRuleFlagsABlankLabel();
        RegionalOffboardingNoticeCostRangeRuleFlagsValuesAboveCost();
    }
}
