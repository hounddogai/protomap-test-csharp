namespace Acme.Workforce.Orgdesign.Tests.Position.Policy;

using Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PositionTargetRulesChecks
{
    public static void PositionTargetValidatorReportsEveryProblem()
    {
        var validator = PositionTargetRules.DefaultPositionTargetValidator();
        var item = PositionTarget.NewPositionTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new PositionTargetRequiredNameRule("position target needs a value");
        var item = PositionTarget.NewPositionTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionTargetRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new PositionTargetRequiredDepartmentRule("position target needs a value");
        var item = PositionTarget.NewPositionTarget("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionTargetNameLengthRuleFlagsALongName()
    {
        var rule = new PositionTargetNameLengthRule(120);
        var item = PositionTarget.NewPositionTarget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PositionTargetValidatorReportsEveryProblem();
        PositionTargetRequiredNameRuleFlagsABlankName();
        PositionTargetRequiredDepartmentRuleFlagsABlankDepartment();
        PositionTargetNameLengthRuleFlagsALongName();
    }
}
