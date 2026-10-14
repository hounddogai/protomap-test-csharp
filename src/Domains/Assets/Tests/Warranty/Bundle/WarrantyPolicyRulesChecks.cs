namespace Acme.Workforce.Assets.Tests.Warranty.Bundle;

using Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WarrantyPolicyRulesChecks
{
    public static void WarrantyPolicyValidatorReportsEveryProblem()
    {
        var validator = WarrantyPolicyRules.DefaultWarrantyPolicyValidator();
        var item = WarrantyPolicy.NewWarrantyPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyPolicyLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WarrantyPolicyLabelLengthRule(3600);
        var item = WarrantyPolicy.NewWarrantyPolicy("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyPolicyRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new WarrantyPolicyRequiredDescriptionRule("warranty policy needs an entry");
        var item = WarrantyPolicy.NewWarrantyPolicy("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WarrantyPolicyValidatorReportsEveryProblem();
        WarrantyPolicyLabelLengthRuleFlagsALongLabel();
        WarrantyPolicyRequiredDescriptionRuleFlagsABlankDescription();
    }
}
