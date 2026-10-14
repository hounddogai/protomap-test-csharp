namespace Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>A rule a warranty checkpoint must pass. Check returns its problems, or none.</summary>
public interface IWarrantyCheckpointRule
{
    List<string> Check(WarrantyCheckpoint item);
}

public sealed record WarrantyCheckpointCodePrefixRule(string Prefix) : IWarrantyCheckpointRule
{
    public List<string> Check(WarrantyCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyCheckpointTimezoneLengthRule(int MaxLength) : IWarrantyCheckpointRule
{
    public List<string> Check(WarrantyCheckpoint item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyCheckpointTitleLengthRule(int MaxLength) : IWarrantyCheckpointRule
{
    public List<string> Check(WarrantyCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty checkpoint.</summary>
public sealed record WarrantyCheckpointValidator(List<IWarrantyCheckpointRule> Rules)
{
    public List<string> Validate(WarrantyCheckpoint item)
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

    public void RequireValid(WarrantyCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyCheckpoint> items)
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

/// <summary>Functions of warranty checkpoint rules.</summary>
public static class WarrantyCheckpointRules
{
    public static WarrantyCheckpointValidator DefaultWarrantyCheckpointValidator()
    {
        return new WarrantyCheckpointValidator(
            new List<IWarrantyCheckpointRule>
            {
                new WarrantyCheckpointCodePrefixRule("OPS"),
                new WarrantyCheckpointTimezoneLengthRule(1000),
                new WarrantyCheckpointTitleLengthRule(16),
            });
    }
}
