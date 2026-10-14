namespace Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>A rule a tiered warranty tier must pass. Check returns its problems, or none.</summary>
public interface ITieredWarrantyTierRule
{
    List<string> Check(TieredWarrantyTier item);
}

public sealed record TieredWarrantyTierRequiredLabelRule(string Message) : ITieredWarrantyTierRule
{
    public List<string> Check(TieredWarrantyTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredWarrantyTierCategoryLengthRule(int MaxLength) : ITieredWarrantyTierRule
{
    public List<string> Check(TieredWarrantyTier item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredWarrantyTierScoreRangeRule(double Min, double Max) : ITieredWarrantyTierRule
{
    public List<string> Check(TieredWarrantyTier item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredWarrantyTierRequiredCategoryRule(string Message) : ITieredWarrantyTierRule
{
    public List<string> Check(TieredWarrantyTier item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredWarrantyTierChannelLengthRule(int MaxLength) : ITieredWarrantyTierRule
{
    public List<string> Check(TieredWarrantyTier item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredWarrantyTierRequiredChannelRule(string Message) : ITieredWarrantyTierRule
{
    public List<string> Check(TieredWarrantyTier item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered warranty tier.</summary>
public sealed record TieredWarrantyTierValidator(List<ITieredWarrantyTierRule> Rules)
{
    public List<string> Validate(TieredWarrantyTier item)
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

    public void RequireValid(TieredWarrantyTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredWarrantyTier> items)
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

/// <summary>Functions of tiered warranty tier rules.</summary>
public static class TieredWarrantyTierRules
{
    public static TieredWarrantyTierValidator DefaultTieredWarrantyTierValidator()
    {
        return new TieredWarrantyTierValidator(
            new List<ITieredWarrantyTierRule>
            {
                new TieredWarrantyTierRequiredLabelRule("tiered warranty tier needs an entry"),
                new TieredWarrantyTierCategoryLengthRule(120),
                new TieredWarrantyTierScoreRangeRule(0.5, 827.4),
                new TieredWarrantyTierRequiredCategoryRule("tiered warranty tier needs an entry"),
                new TieredWarrantyTierChannelLengthRule(32),
                new TieredWarrantyTierRequiredChannelRule("tiered warranty tier needs a value"),
            });
    }
}
