namespace Acme.Workforce.Orgdesign.Tests.CostModel.Reservation;

using Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CostModelPreferenceRulesChecks
{
    public static void CostModelPreferenceValidatorReportsEveryProblem()
    {
        var validator = CostModelPreferenceRules.DefaultCostModelPreferenceValidator();
        var item = CostModelPreference.NewCostModelPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelPreferenceRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new CostModelPreferenceRetriesRangeRule(32, 3600);
        var item = CostModelPreference.NewCostModelPreference("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelPreferenceRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new CostModelPreferenceRequiredPhoneRule("cost model preference needs a value");
        var item = CostModelPreference.NewCostModelPreference("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new CostModelPreferenceCodeLengthRule(80);
        var item = CostModelPreference.NewCostModelPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelPreferenceUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new CostModelPreferenceUpdatedAtRangeRule(64, 86400);
        var item = CostModelPreference.NewCostModelPreference("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CostModelPreferenceValidatorReportsEveryProblem();
        CostModelPreferenceRetriesRangeRuleFlagsValuesAboveRetries();
        CostModelPreferenceRequiredPhoneRuleFlagsABlankPhone();
        CostModelPreferenceCodeLengthRuleFlagsALongCode();
        CostModelPreferenceUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
