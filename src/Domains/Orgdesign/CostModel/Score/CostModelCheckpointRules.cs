namespace Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>A rule a cost model checkpoint must pass. Check returns its problems, or none.</summary>
public interface ICostModelCheckpointRule
{
    List<string> Check(CostModelCheckpoint item);
}

public sealed record CostModelCheckpointRequiredTimezoneRule(string Message) : ICostModelCheckpointRule
{
    public List<string> Check(CostModelCheckpoint item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelCheckpointRequiredNameRule(string Message) : ICostModelCheckpointRule
{
    public List<string> Check(CostModelCheckpoint item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelCheckpointTimezoneLengthRule(int MaxLength) : ICostModelCheckpointRule
{
    public List<string> Check(CostModelCheckpoint item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelCheckpointTitleLengthRule(int MaxLength) : ICostModelCheckpointRule
{
    public List<string> Check(CostModelCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelCheckpointCodePrefixRule(string Prefix) : ICostModelCheckpointRule
{
    public List<string> Check(CostModelCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model checkpoint.</summary>
public sealed record CostModelCheckpointValidator(List<ICostModelCheckpointRule> Rules)
{
    public List<string> Validate(CostModelCheckpoint item)
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

    public void RequireValid(CostModelCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelCheckpoint> items)
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

/// <summary>Functions of cost model checkpoint rules.</summary>
public static class CostModelCheckpointRules
{
    public static CostModelCheckpointValidator DefaultCostModelCheckpointValidator()
    {
        return new CostModelCheckpointValidator(
            new List<ICostModelCheckpointRule>
            {
                new CostModelCheckpointRequiredTimezoneRule("cost model checkpoint needs an entry"),
                new CostModelCheckpointRequiredNameRule("cost model checkpoint needs this field"),
                new CostModelCheckpointTimezoneLengthRule(120),
                new CostModelCheckpointTitleLengthRule(3600),
                new CostModelCheckpointCodePrefixRule("ACM"),
            });
    }
}
