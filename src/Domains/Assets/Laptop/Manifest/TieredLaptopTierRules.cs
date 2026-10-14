namespace Acme.Workforce.Assets.Laptop.Manifest;

/// <summary>A rule a tiered laptop tier must pass. Check returns its problems, or none.</summary>
public interface ITieredLaptopTierRule
{
    List<string> Check(TieredLaptopTier item);
}

public sealed record TieredLaptopTierCodePrefixRule(string Prefix) : ITieredLaptopTierRule
{
    public List<string> Check(TieredLaptopTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredLaptopTierRequiredLabelRule(string Message) : ITieredLaptopTierRule
{
    public List<string> Check(TieredLaptopTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredLaptopTierRatingRangeRule(double Min, double Max) : ITieredLaptopTierRule
{
    public List<string> Check(TieredLaptopTier item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredLaptopTierRequiredContactEmailRule(string Message) : ITieredLaptopTierRule
{
    public List<string> Check(TieredLaptopTier item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredLaptopTierRequiredCodeRule(string Message) : ITieredLaptopTierRule
{
    public List<string> Check(TieredLaptopTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredLaptopTierLabelLengthRule(int MaxLength) : ITieredLaptopTierRule
{
    public List<string> Check(TieredLaptopTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered laptop tier.</summary>
public sealed record TieredLaptopTierValidator(List<ITieredLaptopTierRule> Rules)
{
    public List<string> Validate(TieredLaptopTier item)
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

    public void RequireValid(TieredLaptopTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredLaptopTier> items)
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

/// <summary>Functions of tiered laptop tier rules.</summary>
public static class TieredLaptopTierRules
{
    public static TieredLaptopTierValidator DefaultTieredLaptopTierValidator()
    {
        return new TieredLaptopTierValidator(
            new List<ITieredLaptopTierRule>
            {
                new TieredLaptopTierCodePrefixRule("ACM"),
                new TieredLaptopTierRequiredLabelRule("tiered laptop tier needs an entry"),
                new TieredLaptopTierRatingRangeRule(5.1, 4148.2),
                new TieredLaptopTierRequiredContactEmailRule("tiered laptop tier needs this field"),
                new TieredLaptopTierRequiredCodeRule("tiered laptop tier needs an entry"),
                new TieredLaptopTierLabelLengthRule(3600),
            });
    }
}
