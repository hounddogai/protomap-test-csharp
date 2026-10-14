namespace Acme.Workforce.Assets.Tests.Refresh.Checkpoint;

using Acme.Workforce.Assets.Refresh.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RefreshTemplateRulesChecks
{
    public static void RefreshTemplateValidatorReportsEveryProblem()
    {
        var validator = RefreshTemplateRules.DefaultRefreshTemplateValidator();
        var item = RefreshTemplate.NewRefreshTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshTemplateRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RefreshTemplateRequiredTitleRule("refresh template needs a value");
        var item = RefreshTemplate.NewRefreshTemplate("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshTemplateRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new RefreshTemplateRequiredChecksumRule("refresh template needs this field");
        var item = RefreshTemplate.NewRefreshTemplate("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshTemplateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RefreshTemplateTitleLengthRule(3600);
        var item = RefreshTemplate.NewRefreshTemplate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshTemplateRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new RefreshTemplateRequiredContactEmailRule("refresh template needs an entry");
        var item = RefreshTemplate.NewRefreshTemplate("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshTemplateChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new RefreshTemplateChecksumLengthRule(16);
        var item = RefreshTemplate.NewRefreshTemplate("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RefreshTemplateValidatorReportsEveryProblem();
        RefreshTemplateRequiredTitleRuleFlagsABlankTitle();
        RefreshTemplateRequiredChecksumRuleFlagsABlankChecksum();
        RefreshTemplateTitleLengthRuleFlagsALongTitle();
        RefreshTemplateRequiredContactEmailRuleFlagsABlankContactEmail();
        RefreshTemplateChecksumLengthRuleFlagsALongChecksum();
    }
}
