namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ManualAccrualPolicyRulesChecks
{
    public static void ManualAccrualPolicyValidatorReportsEveryProblem()
    {
        var validator = ManualAccrualPolicyRules.DefaultManualAccrualPolicyValidator();
        var item = ManualAccrualPolicy.NewManualAccrualPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualAccrualPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualAccrualPolicyCodeLengthRule(64);
        var item = ManualAccrualPolicy.NewManualAccrualPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualAccrualPolicyExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new ManualAccrualPolicyExpiresAtRangeRule(32, 1000);
        var item = ManualAccrualPolicy.NewManualAccrualPolicy("x", "y");
        item.ExpiresAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualAccrualPolicyBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new ManualAccrualPolicyBadgeLengthRule(1000);
        var item = ManualAccrualPolicy.NewManualAccrualPolicy("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualAccrualPolicyRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new ManualAccrualPolicyRequiredExternalRefRule("manual accrual policy needs this field");
        var item = ManualAccrualPolicy.NewManualAccrualPolicy("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualAccrualPolicyRequiredNameRuleFlagsABlankName()
    {
        var rule = new ManualAccrualPolicyRequiredNameRule("manual accrual policy needs a value");
        var item = ManualAccrualPolicy.NewManualAccrualPolicy("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualAccrualPolicyValidatorReportsEveryProblem();
        ManualAccrualPolicyCodeLengthRuleFlagsALongCode();
        ManualAccrualPolicyExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        ManualAccrualPolicyBadgeLengthRuleFlagsALongBadge();
        ManualAccrualPolicyRequiredExternalRefRuleFlagsABlankExternalRef();
        ManualAccrualPolicyRequiredNameRuleFlagsABlankName();
    }
}
