namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

/// <summary>A rule a weighted headcount rollup must pass. Check returns its problems, or none.</summary>
public interface IWeightedHeadcountRollupRule
{
    List<string> Check(WeightedHeadcountRollup item);
}

public sealed record WeightedHeadcountRollupCodeLengthRule(int MaxLength) : IWeightedHeadcountRollupRule
{
    public List<string> Check(WeightedHeadcountRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedHeadcountRollupRequiredLocationRule(string Message) : IWeightedHeadcountRollupRule
{
    public List<string> Check(WeightedHeadcountRollup item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedHeadcountRollupRequiredContactEmailRule(string Message) : IWeightedHeadcountRollupRule
{
    public List<string> Check(WeightedHeadcountRollup item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedHeadcountRollupCodePrefixRule(string Prefix) : IWeightedHeadcountRollupRule
{
    public List<string> Check(WeightedHeadcountRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedHeadcountRollupLocationLengthRule(int MaxLength) : IWeightedHeadcountRollupRule
{
    public List<string> Check(WeightedHeadcountRollup item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedHeadcountRollupPinnedGuardRule(int Limit) : IWeightedHeadcountRollupRule
{
    public List<string> Check(WeightedHeadcountRollup item)
    {
        if (item.Pinned && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted headcount rollup.</summary>
public sealed record WeightedHeadcountRollupValidator(List<IWeightedHeadcountRollupRule> Rules)
{
    public List<string> Validate(WeightedHeadcountRollup item)
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

    public void RequireValid(WeightedHeadcountRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedHeadcountRollup> items)
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

/// <summary>Functions of weighted headcount rollup rules.</summary>
public static class WeightedHeadcountRollupRules
{
    public static WeightedHeadcountRollupValidator DefaultWeightedHeadcountRollupValidator()
    {
        return new WeightedHeadcountRollupValidator(
            new List<IWeightedHeadcountRollupRule>
            {
                new WeightedHeadcountRollupCodeLengthRule(255),
                new WeightedHeadcountRollupRequiredLocationRule("weighted headcount rollup needs a value"),
                new WeightedHeadcountRollupRequiredContactEmailRule("weighted headcount rollup needs a value"),
                new WeightedHeadcountRollupCodePrefixRule("X"),
                new WeightedHeadcountRollupLocationLengthRule(1000),
                new WeightedHeadcountRollupPinnedGuardRule(120),
            });
    }
}
