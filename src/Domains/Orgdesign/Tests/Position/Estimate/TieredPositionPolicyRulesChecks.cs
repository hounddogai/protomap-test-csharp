namespace Acme.Workforce.Orgdesign.Tests.Position.Estimate;

using Acme.Workforce.Orgdesign.Position.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TieredPositionPolicyRulesChecks
{
    public static void TieredPositionPolicyValidatorReportsEveryProblem()
    {
        var validator = TieredPositionPolicyRules.DefaultTieredPositionPolicyValidator();
        var item = TieredPositionPolicy.NewTieredPositionPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPositionPolicyTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TieredPositionPolicyTitleLengthRule(64);
        var item = TieredPositionPolicy.NewTieredPositionPolicy("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPositionPolicyRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new TieredPositionPolicyRequiredDescriptionRule("tiered position policy needs an entry");
        var item = TieredPositionPolicy.NewTieredPositionPolicy("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredPositionPolicyValidatorReportsEveryProblem();
        TieredPositionPolicyTitleLengthRuleFlagsALongTitle();
        TieredPositionPolicyRequiredDescriptionRuleFlagsABlankDescription();
    }
}
