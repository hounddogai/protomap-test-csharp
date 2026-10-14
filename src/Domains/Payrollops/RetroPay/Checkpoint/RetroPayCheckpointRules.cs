namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>A rule a retro pay checkpoint must pass. Check returns its problems, or none.</summary>
public interface IRetroPayCheckpointRule
{
    List<string> Check(RetroPayCheckpoint item);
}

public sealed record RetroPayCheckpointRequiredEmployeeIdRule(string Message) : IRetroPayCheckpointRule
{
    public List<string> Check(RetroPayCheckpoint item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayCheckpointVersionRangeRule(int Min, int Max) : IRetroPayCheckpointRule
{
    public List<string> Check(RetroPayCheckpoint item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayCheckpointRevisionRangeRule(int Min, int Max) : IRetroPayCheckpointRule
{
    public List<string> Check(RetroPayCheckpoint item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayCheckpointCodePrefixRule(string Prefix) : IRetroPayCheckpointRule
{
    public List<string> Check(RetroPayCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay checkpoint.</summary>
public sealed record RetroPayCheckpointValidator(List<IRetroPayCheckpointRule> Rules)
{
    public List<string> Validate(RetroPayCheckpoint item)
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

    public void RequireValid(RetroPayCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayCheckpoint> items)
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

/// <summary>Functions of retro pay checkpoint rules.</summary>
public static class RetroPayCheckpointRules
{
    public static RetroPayCheckpointValidator DefaultRetroPayCheckpointValidator()
    {
        return new RetroPayCheckpointValidator(
            new List<IRetroPayCheckpointRule>
            {
                new RetroPayCheckpointRequiredEmployeeIdRule("retro pay checkpoint needs this field"),
                new RetroPayCheckpointVersionRangeRule(16, 80),
                new RetroPayCheckpointRevisionRangeRule(64, 120),
                new RetroPayCheckpointCodePrefixRule("ACM"),
            });
    }
}
