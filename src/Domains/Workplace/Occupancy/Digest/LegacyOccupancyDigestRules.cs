namespace Acme.Workforce.Workplace.Occupancy.Digest;

/// <summary>A rule a legacy occupancy digest must pass. Check returns its problems, or none.</summary>
public interface ILegacyOccupancyDigestRule
{
    List<string> Check(LegacyOccupancyDigest item);
}

public sealed record LegacyOccupancyDigestRateRangeRule(double Min, double Max) : ILegacyOccupancyDigestRule
{
    public List<string> Check(LegacyOccupancyDigest item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOccupancyDigestCurrencyLengthRule(int MaxLength) : ILegacyOccupancyDigestRule
{
    public List<string> Check(LegacyOccupancyDigest item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOccupancyDigestCodePrefixRule(string Prefix) : ILegacyOccupancyDigestRule
{
    public List<string> Check(LegacyOccupancyDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOccupancyDigestMarginRangeRule(double Min, double Max) : ILegacyOccupancyDigestRule
{
    public List<string> Check(LegacyOccupancyDigest item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyOccupancyDigestRatingRangeRule(double Min, double Max) : ILegacyOccupancyDigestRule
{
    public List<string> Check(LegacyOccupancyDigest item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a legacy occupancy digest.</summary>
public sealed record LegacyOccupancyDigestValidator(List<ILegacyOccupancyDigestRule> Rules)
{
    public List<string> Validate(LegacyOccupancyDigest item)
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

    public void RequireValid(LegacyOccupancyDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyOccupancyDigest> items)
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

/// <summary>Functions of legacy occupancy digest rules.</summary>
public static class LegacyOccupancyDigestRules
{
    public static LegacyOccupancyDigestValidator DefaultLegacyOccupancyDigestValidator()
    {
        return new LegacyOccupancyDigestValidator(
            new List<ILegacyOccupancyDigestRule>
            {
                new LegacyOccupancyDigestRateRangeRule(2.4, 3853.0),
                new LegacyOccupancyDigestCurrencyLengthRule(64),
                new LegacyOccupancyDigestCodePrefixRule("REF"),
                new LegacyOccupancyDigestMarginRangeRule(8.3, 3411.7),
                new LegacyOccupancyDigestRatingRangeRule(7.0, 746.5),
            });
    }
}
