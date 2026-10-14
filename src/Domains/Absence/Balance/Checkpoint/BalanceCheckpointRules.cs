namespace Acme.Workforce.Absence.Balance.Checkpoint;

/// <summary>A rule a balance checkpoint must pass. Check returns its problems, or none.</summary>
public interface IBalanceCheckpointRule
{
    List<string> Check(BalanceCheckpoint item);
}

public sealed record BalanceCheckpointCodeLengthRule(int MaxLength) : IBalanceCheckpointRule
{
    public List<string> Check(BalanceCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceCheckpointRequiredNameRule(string Message) : IBalanceCheckpointRule
{
    public List<string> Check(BalanceCheckpoint item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceCheckpointRequiredLabelRule(string Message) : IBalanceCheckpointRule
{
    public List<string> Check(BalanceCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceCheckpointNameLengthRule(int MaxLength) : IBalanceCheckpointRule
{
    public List<string> Check(BalanceCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceCheckpointLabelLengthRule(int MaxLength) : IBalanceCheckpointRule
{
    public List<string> Check(BalanceCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceCheckpointLeaveDaysRangeRule(int Min, int Max) : IBalanceCheckpointRule
{
    public List<string> Check(BalanceCheckpoint item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a balance checkpoint.</summary>
public sealed record BalanceCheckpointValidator(List<IBalanceCheckpointRule> Rules)
{
    public List<string> Validate(BalanceCheckpoint item)
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

    public void RequireValid(BalanceCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceCheckpoint> items)
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

/// <summary>Functions of balance checkpoint rules.</summary>
public static class BalanceCheckpointRules
{
    public static BalanceCheckpointValidator DefaultBalanceCheckpointValidator()
    {
        return new BalanceCheckpointValidator(
            new List<IBalanceCheckpointRule>
            {
                new BalanceCheckpointCodeLengthRule(80),
                new BalanceCheckpointRequiredNameRule("balance checkpoint needs an entry"),
                new BalanceCheckpointRequiredLabelRule("balance checkpoint needs this field"),
                new BalanceCheckpointNameLengthRule(80),
                new BalanceCheckpointLabelLengthRule(32),
                new BalanceCheckpointLeaveDaysRangeRule(32, 86400),
            });
    }
}
