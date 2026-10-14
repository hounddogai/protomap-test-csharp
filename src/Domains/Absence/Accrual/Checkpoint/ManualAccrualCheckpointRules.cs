namespace Acme.Workforce.Absence.Accrual.Checkpoint;

/// <summary>A rule a manual accrual checkpoint must pass. Check returns its problems, or none.</summary>
public interface IManualAccrualCheckpointRule
{
    List<string> Check(ManualAccrualCheckpoint item);
}

public sealed record ManualAccrualCheckpointRequiredLabelRule(string Message) : IManualAccrualCheckpointRule
{
    public List<string> Check(ManualAccrualCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualCheckpointRequiredCodeRule(string Message) : IManualAccrualCheckpointRule
{
    public List<string> Check(ManualAccrualCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualCheckpointRatingRangeRule(double Min, double Max) : IManualAccrualCheckpointRule
{
    public List<string> Check(ManualAccrualCheckpoint item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualCheckpointLabelLengthRule(int MaxLength) : IManualAccrualCheckpointRule
{
    public List<string> Check(ManualAccrualCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualCheckpointRequiredTitleRule(string Message) : IManualAccrualCheckpointRule
{
    public List<string> Check(ManualAccrualCheckpoint item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualCheckpointCodeLengthRule(int MaxLength) : IManualAccrualCheckpointRule
{
    public List<string> Check(ManualAccrualCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual accrual checkpoint.</summary>
public sealed record ManualAccrualCheckpointValidator(List<IManualAccrualCheckpointRule> Rules)
{
    public List<string> Validate(ManualAccrualCheckpoint item)
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

    public void RequireValid(ManualAccrualCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAccrualCheckpoint> items)
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

/// <summary>Functions of manual accrual checkpoint rules.</summary>
public static class ManualAccrualCheckpointRules
{
    public static ManualAccrualCheckpointValidator DefaultManualAccrualCheckpointValidator()
    {
        return new ManualAccrualCheckpointValidator(
            new List<IManualAccrualCheckpointRule>
            {
                new ManualAccrualCheckpointRequiredLabelRule("manual accrual checkpoint needs a value"),
                new ManualAccrualCheckpointRequiredCodeRule("manual accrual checkpoint needs an entry"),
                new ManualAccrualCheckpointRatingRangeRule(3.7, 4027.2),
                new ManualAccrualCheckpointLabelLengthRule(80),
                new ManualAccrualCheckpointRequiredTitleRule("manual accrual checkpoint needs a value"),
                new ManualAccrualCheckpointCodeLengthRule(86400),
            });
    }
}
