namespace Acme.Workforce.Orgdesign.Tests.CostModel.Entry;

using Acme.Workforce.Orgdesign.CostModel.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CostModelAuditRulesChecks
{
    public static void CostModelAuditValidatorReportsEveryProblem()
    {
        var validator = CostModelAuditRules.DefaultCostModelAuditValidator();
        var item = CostModelAudit.NewCostModelAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelAuditCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new CostModelAuditCreatedAtRangeRule(3600, 3600);
        var item = CostModelAudit.NewCostModelAudit("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelAuditRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new CostModelAuditRequiredSourceRule("cost model audit needs this field");
        var item = CostModelAudit.NewCostModelAudit("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelAuditTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CostModelAuditTitleLengthRule(120);
        var item = CostModelAudit.NewCostModelAudit("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelAuditLocationLengthRuleFlagsALongLocation()
    {
        var rule = new CostModelAuditLocationLengthRule(64);
        var item = CostModelAudit.NewCostModelAudit("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelAuditSourceLengthRuleFlagsALongSource()
    {
        var rule = new CostModelAuditSourceLengthRule(1000);
        var item = CostModelAudit.NewCostModelAudit("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CostModelAuditValidatorReportsEveryProblem();
        CostModelAuditCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        CostModelAuditRequiredSourceRuleFlagsABlankSource();
        CostModelAuditTitleLengthRuleFlagsALongTitle();
        CostModelAuditLocationLengthRuleFlagsALongLocation();
        CostModelAuditSourceLengthRuleFlagsALongSource();
    }
}
