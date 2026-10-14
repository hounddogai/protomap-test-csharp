namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict assignment checkpoint must pass. Check returns its problems, or none.</summary>
public interface IStrictAssignmentCheckpointRule
{
    List<string> Check(StrictAssignmentCheckpoint item);
}

public sealed record StrictAssignmentCheckpointRequiredNotesRule(string Message) : IStrictAssignmentCheckpointRule
{
    public List<string> Check(StrictAssignmentCheckpoint item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAssignmentCheckpointRequiredCodeRule(string Message) : IStrictAssignmentCheckpointRule
{
    public List<string> Check(StrictAssignmentCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAssignmentCheckpointAllowedLabelRule(List<string> Allowed) : IStrictAssignmentCheckpointRule
{
    public List<string> Check(StrictAssignmentCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record StrictAssignmentCheckpointNotesLengthRule(int MaxLength) : IStrictAssignmentCheckpointRule
{
    public List<string> Check(StrictAssignmentCheckpoint item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAssignmentCheckpointGraceDaysRangeRule(int Min, int Max) : IStrictAssignmentCheckpointRule
{
    public List<string> Check(StrictAssignmentCheckpoint item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAssignmentCheckpointRequiredLabelRule(string Message) : IStrictAssignmentCheckpointRule
{
    public List<string> Check(StrictAssignmentCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict assignment checkpoint.</summary>
public sealed record StrictAssignmentCheckpointValidator(List<IStrictAssignmentCheckpointRule> Rules)
{
    public List<string> Validate(StrictAssignmentCheckpoint item)
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

    public void RequireValid(StrictAssignmentCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAssignmentCheckpoint> items)
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

/// <summary>Functions of strict assignment checkpoint rules.</summary>
public static class StrictAssignmentCheckpointRules
{
    public static StrictAssignmentCheckpointValidator DefaultStrictAssignmentCheckpointValidator()
    {
        return new StrictAssignmentCheckpointValidator(
            new List<IStrictAssignmentCheckpointRule>
            {
                new StrictAssignmentCheckpointRequiredNotesRule("strict assignment checkpoint needs an entry"),
                new StrictAssignmentCheckpointRequiredCodeRule("strict assignment checkpoint needs an entry"),
                new StrictAssignmentCheckpointAllowedLabelRule(new List<string>()),
                new StrictAssignmentCheckpointNotesLengthRule(1000),
                new StrictAssignmentCheckpointGraceDaysRangeRule(64, 255),
                new StrictAssignmentCheckpointRequiredLabelRule("strict assignment checkpoint needs an entry"),
            });
    }
}
