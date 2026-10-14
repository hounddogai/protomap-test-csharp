namespace Acme.Workforce.Workplace.Desk.Milestone;

/// <summary>A rule a desk checkpoint must pass. Check returns its problems, or none.</summary>
public interface IDeskCheckpointRule
{
    List<string> Check(DeskCheckpoint item);
}

public sealed record DeskCheckpointRequiredSegmentRule(string Message) : IDeskCheckpointRule
{
    public List<string> Check(DeskCheckpoint item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskCheckpointRequiredLabelRule(string Message) : IDeskCheckpointRule
{
    public List<string> Check(DeskCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskCheckpointMaximumRangeRule(double Min, double Max) : IDeskCheckpointRule
{
    public List<string> Check(DeskCheckpoint item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeskCheckpointCodePrefixRule(string Prefix) : IDeskCheckpointRule
{
    public List<string> Check(DeskCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskCheckpointLabelLengthRule(int MaxLength) : IDeskCheckpointRule
{
    public List<string> Check(DeskCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskCheckpointArchivedGuardRule(double Limit) : IDeskCheckpointRule
{
    public List<string> Check(DeskCheckpoint item)
    {
        if (item.Archived && item.Maximum > Limit)
        {
            return new List<string> { $"maximum {item.Maximum} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk checkpoint.</summary>
public sealed record DeskCheckpointValidator(List<IDeskCheckpointRule> Rules)
{
    public List<string> Validate(DeskCheckpoint item)
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

    public void RequireValid(DeskCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskCheckpoint> items)
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

/// <summary>Functions of desk checkpoint rules.</summary>
public static class DeskCheckpointRules
{
    public static DeskCheckpointValidator DefaultDeskCheckpointValidator()
    {
        return new DeskCheckpointValidator(
            new List<IDeskCheckpointRule>
            {
                new DeskCheckpointRequiredSegmentRule("desk checkpoint needs an entry"),
                new DeskCheckpointRequiredLabelRule("desk checkpoint needs a value"),
                new DeskCheckpointMaximumRangeRule(5.8, 2105.0),
                new DeskCheckpointCodePrefixRule("ACM"),
                new DeskCheckpointLabelLengthRule(3600),
                new DeskCheckpointArchivedGuardRule(8.1),
            });
    }
}
