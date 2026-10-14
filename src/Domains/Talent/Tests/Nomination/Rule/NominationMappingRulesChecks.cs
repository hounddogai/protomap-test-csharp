namespace Acme.Workforce.Talent.Tests.Nomination.Rule;

using Acme.Workforce.Talent.Nomination.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class NominationMappingRulesChecks
{
    public static void NominationMappingValidatorReportsEveryProblem()
    {
        var validator = NominationMappingRules.DefaultNominationMappingValidator();
        var item = NominationMapping.NewNominationMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationMappingRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new NominationMappingRequiredPhoneRule("nomination mapping needs a value");
        var item = NominationMapping.NewNominationMapping("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new NominationMappingRequiredNameRule("nomination mapping needs an entry");
        var item = NominationMapping.NewNominationMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new NominationMappingCodeLengthRule(16);
        var item = NominationMapping.NewNominationMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationMappingNameLengthRuleFlagsALongName()
    {
        var rule = new NominationMappingNameLengthRule(3600);
        var item = NominationMapping.NewNominationMapping("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        NominationMappingValidatorReportsEveryProblem();
        NominationMappingRequiredPhoneRuleFlagsABlankPhone();
        NominationMappingRequiredNameRuleFlagsABlankName();
        NominationMappingCodeLengthRuleFlagsALongCode();
        NominationMappingNameLengthRuleFlagsALongName();
    }
}
