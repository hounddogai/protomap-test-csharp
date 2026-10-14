namespace Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>A rule a visitor checkpoint must pass. Check returns its problems, or none.</summary>
public interface IVisitorCheckpointRule
{
    List<string> Check(VisitorCheckpoint item);
}

public sealed record VisitorCheckpointCodePrefixRule(string Prefix) : IVisitorCheckpointRule
{
    public List<string> Check(VisitorCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorCheckpointQuantityRangeRule(int Min, int Max) : IVisitorCheckpointRule
{
    public List<string> Check(VisitorCheckpoint item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorCheckpointRequiredTitleRule(string Message) : IVisitorCheckpointRule
{
    public List<string> Check(VisitorCheckpoint item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor checkpoint.</summary>
public sealed record VisitorCheckpointValidator(List<IVisitorCheckpointRule> Rules)
{
    public List<string> Validate(VisitorCheckpoint item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(VisitorCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorCheckpoint> items)
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

/// <summary>Functions of visitor checkpoint rules.</summary>
public static class VisitorCheckpointRules
{
    public static VisitorCheckpointValidator DefaultVisitorCheckpointValidator()
    {
        return new VisitorCheckpointValidator(
            new List<IVisitorCheckpointRule>
            {
                new VisitorCheckpointCodePrefixRule("X"),
                new VisitorCheckpointQuantityRangeRule(16, 86400),
                new VisitorCheckpointRequiredTitleRule("visitor checkpoint needs a value"),
            });
    }
}
