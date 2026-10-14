namespace Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>A rule a exception checkpoint must pass. Check returns its problems, or none.</summary>
public interface IExceptionCheckpointRule
{
    List<string> Check(ExceptionCheckpoint item);
}

public sealed record ExceptionCheckpointRequiredLabelRule(string Message) : IExceptionCheckpointRule
{
    public List<string> Check(ExceptionCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionCheckpointThresholdRangeRule(double Min, double Max) : IExceptionCheckpointRule
{
    public List<string> Check(ExceptionCheckpoint item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionCheckpointFloorValueRangeRule(double Min, double Max) : IExceptionCheckpointRule
{
    public List<string> Check(ExceptionCheckpoint item)
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

public sealed record ExceptionCheckpointLabelLengthRule(int MaxLength) : IExceptionCheckpointRule
{
    public List<string> Check(ExceptionCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception checkpoint.</summary>
public sealed record ExceptionCheckpointValidator(List<IExceptionCheckpointRule> Rules)
{
    public List<string> Validate(ExceptionCheckpoint item)
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

    public void RequireValid(ExceptionCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionCheckpoint> items)
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

/// <summary>Functions of exception checkpoint rules.</summary>
public static class ExceptionCheckpointRules
{
    public static ExceptionCheckpointValidator DefaultExceptionCheckpointValidator()
    {
        return new ExceptionCheckpointValidator(
            new List<IExceptionCheckpointRule>
            {
                new ExceptionCheckpointRequiredLabelRule("exception checkpoint needs a value"),
                new ExceptionCheckpointThresholdRangeRule(4.2, 3033.1),
                new ExceptionCheckpointFloorValueRangeRule(2.1, 150.9),
                new ExceptionCheckpointLabelLengthRule(64),
            });
    }
}
