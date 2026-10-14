namespace Acme.Workforce.Compensation.BonusPool.Group;

/// <summary>A rule a adaptive bonus pool checkpoint must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveBonusPoolCheckpointRule
{
    List<string> Check(AdaptiveBonusPoolCheckpoint item);
}

public sealed record AdaptiveBonusPoolCheckpointCodePrefixRule(string Prefix) : IAdaptiveBonusPoolCheckpointRule
{
    public List<string> Check(AdaptiveBonusPoolCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBonusPoolCheckpointLocationLengthRule(int MaxLength) : IAdaptiveBonusPoolCheckpointRule
{
    public List<string> Check(AdaptiveBonusPoolCheckpoint item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBonusPoolCheckpointRequiredLocationRule(string Message) : IAdaptiveBonusPoolCheckpointRule
{
    public List<string> Check(AdaptiveBonusPoolCheckpoint item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBonusPoolCheckpointRequiredChecksumRule(string Message) : IAdaptiveBonusPoolCheckpointRule
{
    public List<string> Check(AdaptiveBonusPoolCheckpoint item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive bonus pool checkpoint.</summary>
public sealed record AdaptiveBonusPoolCheckpointValidator(List<IAdaptiveBonusPoolCheckpointRule> Rules)
{
    public List<string> Validate(AdaptiveBonusPoolCheckpoint item)
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

    public void RequireValid(AdaptiveBonusPoolCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveBonusPoolCheckpoint> items)
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

/// <summary>Functions of adaptive bonus pool checkpoint rules.</summary>
public static class AdaptiveBonusPoolCheckpointRules
{
    public static AdaptiveBonusPoolCheckpointValidator DefaultAdaptiveBonusPoolCheckpointValidator()
    {
        return new AdaptiveBonusPoolCheckpointValidator(
            new List<IAdaptiveBonusPoolCheckpointRule>
            {
                new AdaptiveBonusPoolCheckpointCodePrefixRule("OPS"),
                new AdaptiveBonusPoolCheckpointLocationLengthRule(255),
                new AdaptiveBonusPoolCheckpointRequiredLocationRule("adaptive bonus pool checkpoint needs a value"),
                new AdaptiveBonusPoolCheckpointRequiredChecksumRule("adaptive bonus pool checkpoint needs an entry"),
            });
    }
}
