namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

/// <summary>A rule a seasonal warranty tier must pass. Check returns its problems, or none.</summary>
public interface ISeasonalWarrantyTierRule
{
    List<string> Check(SeasonalWarrantyTier item);
}

public sealed record SeasonalWarrantyTierRequiredCodeRule(string Message) : ISeasonalWarrantyTierRule
{
    public List<string> Check(SeasonalWarrantyTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalWarrantyTierCodeLengthRule(int MaxLength) : ISeasonalWarrantyTierRule
{
    public List<string> Check(SeasonalWarrantyTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalWarrantyTierMarginRangeRule(double Min, double Max) : ISeasonalWarrantyTierRule
{
    public List<string> Check(SeasonalWarrantyTier item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal warranty tier.</summary>
public sealed record SeasonalWarrantyTierValidator(List<ISeasonalWarrantyTierRule> Rules)
{
    public List<string> Validate(SeasonalWarrantyTier item)
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

    public void RequireValid(SeasonalWarrantyTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalWarrantyTier> items)
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

/// <summary>Functions of seasonal warranty tier rules.</summary>
public static class SeasonalWarrantyTierRules
{
    public static SeasonalWarrantyTierValidator DefaultSeasonalWarrantyTierValidator()
    {
        return new SeasonalWarrantyTierValidator(
            new List<ISeasonalWarrantyTierRule>
            {
                new SeasonalWarrantyTierRequiredCodeRule("seasonal warranty tier needs a value"),
                new SeasonalWarrantyTierCodeLengthRule(3600),
                new SeasonalWarrantyTierMarginRangeRule(3.5, 699.6),
            });
    }
}
