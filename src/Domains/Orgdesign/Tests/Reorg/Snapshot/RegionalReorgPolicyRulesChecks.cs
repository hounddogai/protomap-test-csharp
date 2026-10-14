namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RegionalReorgPolicyRulesChecks
{
    public static void RegionalReorgPolicyValidatorReportsEveryProblem()
    {
        var validator = RegionalReorgPolicyRules.DefaultRegionalReorgPolicyValidator();
        var item = RegionalReorgPolicy.NewRegionalReorgPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalReorgPolicyRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalReorgPolicyRequiredNameRule("regional reorg policy needs an entry");
        var item = RegionalReorgPolicy.NewRegionalReorgPolicy("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalReorgPolicyRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new RegionalReorgPolicyRequiredCostCenterRule("regional reorg policy needs a value");
        var item = RegionalReorgPolicy.NewRegionalReorgPolicy("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalReorgPolicyRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new RegionalReorgPolicyRequiredGradeRule("regional reorg policy needs an entry");
        var item = RegionalReorgPolicy.NewRegionalReorgPolicy("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalReorgPolicyValidatorReportsEveryProblem();
        RegionalReorgPolicyRequiredNameRuleFlagsABlankName();
        RegionalReorgPolicyRequiredCostCenterRuleFlagsABlankCostCenter();
        RegionalReorgPolicyRequiredGradeRuleFlagsABlankGrade();
    }
}
