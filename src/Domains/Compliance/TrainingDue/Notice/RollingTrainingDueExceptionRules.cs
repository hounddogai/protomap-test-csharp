namespace Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>A rule a rolling training due exception must pass. Check returns its problems, or none.</summary>
public interface IRollingTrainingDueExceptionRule
{
    List<string> Check(RollingTrainingDueException item);
}

public sealed record RollingTrainingDueExceptionTimezoneLengthRule(int MaxLength) : IRollingTrainingDueExceptionRule
{
    public List<string> Check(RollingTrainingDueException item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingTrainingDueExceptionCreatedAtRangeRule(int Min, int Max) : IRollingTrainingDueExceptionRule
{
    public List<string> Check(RollingTrainingDueException item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingTrainingDueExceptionMarginRangeRule(double Min, double Max) : IRollingTrainingDueExceptionRule
{
    public List<string> Check(RollingTrainingDueException item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling training due exception.</summary>
public sealed record RollingTrainingDueExceptionValidator(List<IRollingTrainingDueExceptionRule> Rules)
{
    public List<string> Validate(RollingTrainingDueException item)
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

    public void RequireValid(RollingTrainingDueException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingTrainingDueException> items)
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

/// <summary>Functions of rolling training due exception rules.</summary>
public static class RollingTrainingDueExceptionRules
{
    public static RollingTrainingDueExceptionValidator DefaultRollingTrainingDueExceptionValidator()
    {
        return new RollingTrainingDueExceptionValidator(
            new List<IRollingTrainingDueExceptionRule>
            {
                new RollingTrainingDueExceptionTimezoneLengthRule(80),
                new RollingTrainingDueExceptionCreatedAtRangeRule(80, 3600),
                new RollingTrainingDueExceptionMarginRangeRule(8.3, 2007.7),
            });
    }
}
