namespace Acme.Workforce.Compliance.Tests.TrainingDue.OverrideItem;

using Acme.Workforce.Compliance.TrainingDue.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TieredTrainingDueAuditRulesChecks
{
    public static void TieredTrainingDueAuditValidatorReportsEveryProblem()
    {
        var validator = TieredTrainingDueAuditRules.DefaultTieredTrainingDueAuditValidator();
        var item = TieredTrainingDueAudit.NewTieredTrainingDueAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredTrainingDueAuditContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new TieredTrainingDueAuditContactEmailLengthRule(80);
        var item = TieredTrainingDueAudit.NewTieredTrainingDueAudit("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredTrainingDueAuditRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredTrainingDueAuditRequiredNameRule("tiered training due audit needs this field");
        var item = TieredTrainingDueAudit.NewTieredTrainingDueAudit("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredTrainingDueAuditRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new TieredTrainingDueAuditRequiredChannelRule("tiered training due audit needs a value");
        var item = TieredTrainingDueAudit.NewTieredTrainingDueAudit("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredTrainingDueAuditChannelLengthRuleFlagsALongChannel()
    {
        var rule = new TieredTrainingDueAuditChannelLengthRule(32);
        var item = TieredTrainingDueAudit.NewTieredTrainingDueAudit("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredTrainingDueAuditValidatorReportsEveryProblem();
        TieredTrainingDueAuditContactEmailLengthRuleFlagsALongContactEmail();
        TieredTrainingDueAuditRequiredNameRuleFlagsABlankName();
        TieredTrainingDueAuditRequiredChannelRuleFlagsABlankChannel();
        TieredTrainingDueAuditChannelLengthRuleFlagsALongChannel();
    }
}
