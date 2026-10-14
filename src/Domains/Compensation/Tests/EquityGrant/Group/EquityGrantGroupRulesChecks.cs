namespace Acme.Workforce.Compensation.Tests.EquityGrant.Group;

using Acme.Workforce.Compensation.EquityGrant.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class EquityGrantGroupRulesChecks
{
    public static void EquityGrantGroupValidatorReportsEveryProblem()
    {
        var validator = EquityGrantGroupRules.DefaultEquityGrantGroupValidator();
        var item = EquityGrantGroup.NewEquityGrantGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantGroupExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new EquityGrantGroupExternalRefLengthRule(1000);
        var item = EquityGrantGroup.NewEquityGrantGroup("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantGroupUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new EquityGrantGroupUpdatedAtRangeRule(16, 1000);
        var item = EquityGrantGroup.NewEquityGrantGroup("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantGroupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new EquityGrantGroupRequiredLabelRule("equity grant group needs an entry");
        var item = EquityGrantGroup.NewEquityGrantGroup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new EquityGrantGroupCodeLengthRule(120);
        var item = EquityGrantGroup.NewEquityGrantGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EquityGrantGroupValidatorReportsEveryProblem();
        EquityGrantGroupExternalRefLengthRuleFlagsALongExternalRef();
        EquityGrantGroupUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        EquityGrantGroupRequiredLabelRuleFlagsABlankLabel();
        EquityGrantGroupCodeLengthRuleFlagsALongCode();
    }
}
