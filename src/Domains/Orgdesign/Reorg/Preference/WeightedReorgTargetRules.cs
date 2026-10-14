namespace Acme.Workforce.Orgdesign.Reorg.Preference;

/// <summary>A rule a weighted reorg target must pass. Check returns its problems, or none.</summary>
public interface IWeightedReorgTargetRule
{
    List<string> Check(WeightedReorgTarget item);
}

public sealed record WeightedReorgTargetRequiredLabelRule(string Message) : IWeightedReorgTargetRule
{
    public List<string> Check(WeightedReorgTarget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedReorgTargetLabelLengthRule(int MaxLength) : IWeightedReorgTargetRule
{
    public List<string> Check(WeightedReorgTarget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedReorgTargetApprovedGuardRule(int Limit) : IWeightedReorgTargetRule
{
    public List<string> Check(WeightedReorgTarget item)
    {
        if (item.Approved && item.ExpiresAt > Limit)
        {
            return new List<string> { $"expires at {item.ExpiresAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record WeightedReorgTargetCodePrefixRule(string Prefix) : IWeightedReorgTargetRule
{
    public List<string> Check(WeightedReorgTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted reorg target.</summary>
public sealed record WeightedReorgTargetValidator(List<IWeightedReorgTargetRule> Rules)
{
    public List<string> Validate(WeightedReorgTarget item)
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

    public void RequireValid(WeightedReorgTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedReorgTarget> items)
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

/// <summary>Functions of weighted reorg target rules.</summary>
public static class WeightedReorgTargetRules
{
    public static WeightedReorgTargetValidator DefaultWeightedReorgTargetValidator()
    {
        return new WeightedReorgTargetValidator(
            new List<IWeightedReorgTargetRule>
            {
                new WeightedReorgTargetRequiredLabelRule("weighted reorg target needs a value"),
                new WeightedReorgTargetLabelLengthRule(120),
                new WeightedReorgTargetApprovedGuardRule(86400),
                new WeightedReorgTargetCodePrefixRule("REF"),
            });
    }
}
