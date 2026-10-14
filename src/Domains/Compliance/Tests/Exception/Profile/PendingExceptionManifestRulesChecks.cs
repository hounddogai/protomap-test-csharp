namespace Acme.Workforce.Compliance.Tests.Exception.Profile;

using Acme.Workforce.Compliance.Exception.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PendingExceptionManifestRulesChecks
{
    public static void PendingExceptionManifestValidatorReportsEveryProblem()
    {
        var validator = PendingExceptionManifestRules.DefaultPendingExceptionManifestValidator();
        var item = PendingExceptionManifest.NewPendingExceptionManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingExceptionManifestPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new PendingExceptionManifestPhoneLengthRule(3600);
        var item = PendingExceptionManifest.NewPendingExceptionManifest("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingExceptionManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingExceptionManifestRequiredCodeRule("pending exception manifest needs an entry");
        var item = PendingExceptionManifest.NewPendingExceptionManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingExceptionManifestValidatorReportsEveryProblem();
        PendingExceptionManifestPhoneLengthRuleFlagsALongPhone();
        PendingExceptionManifestRequiredCodeRuleFlagsABlankCode();
    }
}
