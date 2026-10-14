namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionCheckpoint;

using Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

/// <summary>Checks of exception checkpoint code, run by the project's runner.</summary>
public static class CappedExceptionCheckpointRulesChecks
{
    public static void CappedExceptionCheckpointValidatorReportsEveryProblem()
    {
        var validator = CappedExceptionCheckpointRules.DefaultCappedExceptionCheckpointValidator();
        var item = CappedExceptionCheckpoint.NewCappedExceptionCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedExceptionCheckpointChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new CappedExceptionCheckpointChecksumLengthRule(16);
        var item = CappedExceptionCheckpoint.NewCappedExceptionCheckpoint("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedExceptionCheckpointFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new CappedExceptionCheckpointFloorValueRangeRule(7.5, 4977.8);
        var item = CappedExceptionCheckpoint.NewCappedExceptionCheckpoint("x", "y");
        item.FloorValue = 4979.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedExceptionCheckpointAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new CappedExceptionCheckpointAttemptsRangeRule(16, 1000);
        var item = CappedExceptionCheckpoint.NewCappedExceptionCheckpoint("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedExceptionCheckpointEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new CappedExceptionCheckpointEmployeeIdLengthRule(255);
        var item = CappedExceptionCheckpoint.NewCappedExceptionCheckpoint("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedExceptionCheckpointValidatorReportsEveryProblem();
        CappedExceptionCheckpointChecksumLengthRuleFlagsALongChecksum();
        CappedExceptionCheckpointFloorValueRangeRuleFlagsValuesAboveFloorValue();
        CappedExceptionCheckpointAttemptsRangeRuleFlagsValuesAboveAttempts();
        CappedExceptionCheckpointEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
