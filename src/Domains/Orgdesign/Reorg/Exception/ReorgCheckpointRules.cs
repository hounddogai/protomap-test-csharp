namespace Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>A rule a reorg checkpoint must pass. Check returns its problems, or none.</summary>
public interface IReorgCheckpointRule
{
    List<string> Check(ReorgCheckpoint item);
}

public sealed record ReorgCheckpointRequiredNameRule(string Message) : IReorgCheckpointRule
{
    public List<string> Check(ReorgCheckpoint item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgCheckpointSourceLengthRule(int MaxLength) : IReorgCheckpointRule
{
    public List<string> Check(ReorgCheckpoint item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgCheckpointRequiredSourceRule(string Message) : IReorgCheckpointRule
{
    public List<string> Check(ReorgCheckpoint item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgCheckpointNameLengthRule(int MaxLength) : IReorgCheckpointRule
{
    public List<string> Check(ReorgCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgCheckpointRequiredCodeRule(string Message) : IReorgCheckpointRule
{
    public List<string> Check(ReorgCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg checkpoint.</summary>
public sealed record ReorgCheckpointValidator(List<IReorgCheckpointRule> Rules)
{
    public List<string> Validate(ReorgCheckpoint item)
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

    public void RequireValid(ReorgCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgCheckpoint> items)
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

/// <summary>Functions of reorg checkpoint rules.</summary>
public static class ReorgCheckpointRules
{
    public static ReorgCheckpointValidator DefaultReorgCheckpointValidator()
    {
        return new ReorgCheckpointValidator(
            new List<IReorgCheckpointRule>
            {
                new ReorgCheckpointRequiredNameRule("reorg checkpoint needs a value"),
                new ReorgCheckpointSourceLengthRule(1000),
                new ReorgCheckpointRequiredSourceRule("reorg checkpoint needs a value"),
                new ReorgCheckpointNameLengthRule(1000),
                new ReorgCheckpointRequiredCodeRule("reorg checkpoint needs a value"),
            });
    }
}
