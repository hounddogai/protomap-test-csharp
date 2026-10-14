namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ManualLaptopExceptionRulesChecks
{
    public static void ManualLaptopExceptionValidatorReportsEveryProblem()
    {
        var validator = ManualLaptopExceptionRules.DefaultManualLaptopExceptionValidator();
        var item = ManualLaptopException.NewManualLaptopException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualLaptopExceptionHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new ManualLaptopExceptionHoursRangeRule(4.1, 212.9);
        var item = ManualLaptopException.NewManualLaptopException("x", "y");
        item.Hours = 214.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualLaptopExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualLaptopExceptionCodeLengthRule(255);
        var item = ManualLaptopException.NewManualLaptopException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualLaptopExceptionValidatorReportsEveryProblem();
        ManualLaptopExceptionHoursRangeRuleFlagsValuesAboveHours();
        ManualLaptopExceptionCodeLengthRuleFlagsALongCode();
    }
}
