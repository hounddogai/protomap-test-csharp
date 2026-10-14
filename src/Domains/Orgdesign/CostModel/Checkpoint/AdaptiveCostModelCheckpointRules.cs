namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>A rule a adaptive cost model checkpoint must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveCostModelCheckpointRule
{
    List<string> Check(AdaptiveCostModelCheckpoint item);
}

public sealed record AdaptiveCostModelCheckpointRequiredNameRule(string Message) : IAdaptiveCostModelCheckpointRule
{
    public List<string> Check(AdaptiveCostModelCheckpoint item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCostModelCheckpointCodePrefixRule(string Prefix) : IAdaptiveCostModelCheckpointRule
{
    public List<string> Check(AdaptiveCostModelCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCostModelCheckpointGraceDaysRangeRule(int Min, int Max) : IAdaptiveCostModelCheckpointRule
{
    public List<string> Check(AdaptiveCostModelCheckpoint item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCostModelCheckpointWindowDaysRangeRule(int Min, int Max) : IAdaptiveCostModelCheckpointRule
{
    public List<string> Check(AdaptiveCostModelCheckpoint item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive cost model checkpoint.</summary>
public sealed record AdaptiveCostModelCheckpointValidator(List<IAdaptiveCostModelCheckpointRule> Rules)
{
    public List<string> Validate(AdaptiveCostModelCheckpoint item)
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

    public void RequireValid(AdaptiveCostModelCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveCostModelCheckpoint> items)
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

/// <summary>Functions of adaptive cost model checkpoint rules.</summary>
public static class AdaptiveCostModelCheckpointRules
{
    public static AdaptiveCostModelCheckpointValidator DefaultAdaptiveCostModelCheckpointValidator()
    {
        return new AdaptiveCostModelCheckpointValidator(
            new List<IAdaptiveCostModelCheckpointRule>
            {
                new AdaptiveCostModelCheckpointRequiredNameRule("adaptive cost model checkpoint needs a value"),
                new AdaptiveCostModelCheckpointCodePrefixRule("OPS"),
                new AdaptiveCostModelCheckpointGraceDaysRangeRule(16, 120),
                new AdaptiveCostModelCheckpointWindowDaysRangeRule(16, 16),
            });
    }
}
