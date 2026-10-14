namespace Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>A rule a seasonal warranty profile must pass. Check returns its problems, or none.</summary>
public interface ISeasonalWarrantyProfileRule
{
    List<string> Check(SeasonalWarrantyProfile item);
}

public sealed record SeasonalWarrantyProfileCodePrefixRule(string Prefix) : ISeasonalWarrantyProfileRule
{
    public List<string> Check(SeasonalWarrantyProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalWarrantyProfileOwnerIdLengthRule(int MaxLength) : ISeasonalWarrantyProfileRule
{
    public List<string> Check(SeasonalWarrantyProfile item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalWarrantyProfileLimitRangeRule(int Min, int Max) : ISeasonalWarrantyProfileRule
{
    public List<string> Check(SeasonalWarrantyProfile item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a seasonal warranty profile.</summary>
public sealed record SeasonalWarrantyProfileValidator(List<ISeasonalWarrantyProfileRule> Rules)
{
    public List<string> Validate(SeasonalWarrantyProfile item)
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

    public void RequireValid(SeasonalWarrantyProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalWarrantyProfile> items)
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

/// <summary>Functions of seasonal warranty profile rules.</summary>
public static class SeasonalWarrantyProfileRules
{
    public static SeasonalWarrantyProfileValidator DefaultSeasonalWarrantyProfileValidator()
    {
        return new SeasonalWarrantyProfileValidator(
            new List<ISeasonalWarrantyProfileRule>
            {
                new SeasonalWarrantyProfileCodePrefixRule("REF"),
                new SeasonalWarrantyProfileOwnerIdLengthRule(1000),
                new SeasonalWarrantyProfileLimitRangeRule(32, 64),
            });
    }
}
