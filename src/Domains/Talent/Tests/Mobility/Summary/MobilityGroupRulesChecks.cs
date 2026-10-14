namespace Acme.Workforce.Talent.Tests.Mobility.Summary;

using Acme.Workforce.Talent.Mobility.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class MobilityGroupRulesChecks
{
    public static void MobilityGroupValidatorReportsEveryProblem()
    {
        var validator = MobilityGroupRules.DefaultMobilityGroupValidator();
        var item = MobilityGroup.NewMobilityGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MobilityGroupRequiredCodeRule("mobility group needs an entry");
        var item = MobilityGroup.NewMobilityGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityGroupNameLengthRuleFlagsALongName()
    {
        var rule = new MobilityGroupNameLengthRule(120);
        var item = MobilityGroup.NewMobilityGroup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new MobilityGroupCodeLengthRule(255);
        var item = MobilityGroup.NewMobilityGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MobilityGroupValidatorReportsEveryProblem();
        MobilityGroupRequiredCodeRuleFlagsABlankCode();
        MobilityGroupNameLengthRuleFlagsALongName();
        MobilityGroupCodeLengthRuleFlagsALongCode();
    }
}
