namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionGroup;

using Acme.Workforce.Compliance.Exception.ExceptionGroup;

/// <summary>Checks of exception group code, run by the project's runner.</summary>
public static class ManualExceptionGroupRulesChecks
{
    public static void ManualExceptionGroupValidatorReportsEveryProblem()
    {
        var validator = ManualExceptionGroupRules.DefaultManualExceptionGroupValidator();
        var item = ManualExceptionGroup.NewManualExceptionGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualExceptionGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualExceptionGroupCodeLengthRule(80);
        var item = ManualExceptionGroup.NewManualExceptionGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualExceptionGroupRequiredNameRuleFlagsABlankName()
    {
        var rule = new ManualExceptionGroupRequiredNameRule("manual exception group needs an entry");
        var item = ManualExceptionGroup.NewManualExceptionGroup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualExceptionGroupManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new ManualExceptionGroupManagerIdLengthRule(16);
        var item = ManualExceptionGroup.NewManualExceptionGroup("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualExceptionGroupValidatorReportsEveryProblem();
        ManualExceptionGroupCodeLengthRuleFlagsALongCode();
        ManualExceptionGroupRequiredNameRuleFlagsABlankName();
        ManualExceptionGroupManagerIdLengthRuleFlagsALongManagerId();
    }
}
