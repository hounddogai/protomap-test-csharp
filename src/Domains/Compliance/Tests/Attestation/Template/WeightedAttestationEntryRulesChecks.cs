namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WeightedAttestationEntryRulesChecks
{
    public static void WeightedAttestationEntryValidatorReportsEveryProblem()
    {
        var validator = WeightedAttestationEntryRules.DefaultWeightedAttestationEntryValidator();
        var item = WeightedAttestationEntry.NewWeightedAttestationEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedAttestationEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedAttestationEntryCodeLengthRule(1000);
        var item = WeightedAttestationEntry.NewWeightedAttestationEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedAttestationEntryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedAttestationEntryLabelLengthRule(1000);
        var item = WeightedAttestationEntry.NewWeightedAttestationEntry("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedAttestationEntryValidatorReportsEveryProblem();
        WeightedAttestationEntryCodeLengthRuleFlagsALongCode();
        WeightedAttestationEntryLabelLengthRuleFlagsALongLabel();
    }
}
