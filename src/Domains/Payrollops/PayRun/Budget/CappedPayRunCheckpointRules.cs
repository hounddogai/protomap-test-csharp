namespace Acme.Workforce.Payrollops.PayRun.Budget;

/// <summary>A rule a capped pay run checkpoint must pass. Check returns its problems, or none.</summary>
public interface ICappedPayRunCheckpointRule
{
    List<string> Check(CappedPayRunCheckpoint item);
}

public sealed record CappedPayRunCheckpointTitleLengthRule(int MaxLength) : ICappedPayRunCheckpointRule
{
    public List<string> Check(CappedPayRunCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunCheckpointRequiredRegionRule(string Message) : ICappedPayRunCheckpointRule
{
    public List<string> Check(CappedPayRunCheckpoint item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunCheckpointRequiredTitleRule(string Message) : ICappedPayRunCheckpointRule
{
    public List<string> Check(CappedPayRunCheckpoint item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunCheckpointVersionRangeRule(int Min, int Max) : ICappedPayRunCheckpointRule
{
    public List<string> Check(CappedPayRunCheckpoint item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunCheckpointCodePrefixRule(string Prefix) : ICappedPayRunCheckpointRule
{
    public List<string> Check(CappedPayRunCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped pay run checkpoint.</summary>
public sealed record CappedPayRunCheckpointValidator(List<ICappedPayRunCheckpointRule> Rules)
{
    public List<string> Validate(CappedPayRunCheckpoint item)
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

    public void RequireValid(CappedPayRunCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPayRunCheckpoint> items)
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

/// <summary>Functions of capped pay run checkpoint rules.</summary>
public static class CappedPayRunCheckpointRules
{
    public static CappedPayRunCheckpointValidator DefaultCappedPayRunCheckpointValidator()
    {
        return new CappedPayRunCheckpointValidator(
            new List<ICappedPayRunCheckpointRule>
            {
                new CappedPayRunCheckpointTitleLengthRule(1000),
                new CappedPayRunCheckpointRequiredRegionRule("capped pay run checkpoint needs an entry"),
                new CappedPayRunCheckpointRequiredTitleRule("capped pay run checkpoint needs this field"),
                new CappedPayRunCheckpointVersionRangeRule(1000, 1000),
                new CappedPayRunCheckpointCodePrefixRule("OPS"),
            });
    }
}
