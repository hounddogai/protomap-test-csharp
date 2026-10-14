namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

/// <summary>A rule a capped exception checkpoint must pass. Check returns its problems, or none.</summary>
public interface ICappedExceptionCheckpointRule
{
    List<string> Check(CappedExceptionCheckpoint item);
}

public sealed record CappedExceptionCheckpointChecksumLengthRule(int MaxLength) : ICappedExceptionCheckpointRule
{
    public List<string> Check(CappedExceptionCheckpoint item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedExceptionCheckpointFloorValueRangeRule(double Min, double Max) : ICappedExceptionCheckpointRule
{
    public List<string> Check(CappedExceptionCheckpoint item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedExceptionCheckpointAttemptsRangeRule(int Min, int Max) : ICappedExceptionCheckpointRule
{
    public List<string> Check(CappedExceptionCheckpoint item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedExceptionCheckpointEmployeeIdLengthRule(int MaxLength) : ICappedExceptionCheckpointRule
{
    public List<string> Check(CappedExceptionCheckpoint item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped exception checkpoint.</summary>
public sealed record CappedExceptionCheckpointValidator(List<ICappedExceptionCheckpointRule> Rules)
{
    public List<string> Validate(CappedExceptionCheckpoint item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(CappedExceptionCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedExceptionCheckpoint> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of capped exception checkpoint rules.</summary>
public static class CappedExceptionCheckpointRules
{
    public static CappedExceptionCheckpointValidator DefaultCappedExceptionCheckpointValidator()
    {
        return new CappedExceptionCheckpointValidator(
            new List<ICappedExceptionCheckpointRule>
            {
                new CappedExceptionCheckpointChecksumLengthRule(32),
                new CappedExceptionCheckpointFloorValueRangeRule(1.1, 3903.6),
                new CappedExceptionCheckpointAttemptsRangeRule(120, 120),
                new CappedExceptionCheckpointEmployeeIdLengthRule(80),
            });
    }
}
