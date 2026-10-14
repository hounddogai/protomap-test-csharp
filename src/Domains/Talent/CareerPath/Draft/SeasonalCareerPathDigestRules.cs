namespace Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>A rule a seasonal career path digest must pass. Check returns its problems, or none.</summary>
public interface ISeasonalCareerPathDigestRule
{
    List<string> Check(SeasonalCareerPathDigest item);
}

public sealed record SeasonalCareerPathDigestCodePrefixRule(string Prefix) : ISeasonalCareerPathDigestRule
{
    public List<string> Check(SeasonalCareerPathDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCareerPathDigestRequiredCodeRule(string Message) : ISeasonalCareerPathDigestRule
{
    public List<string> Check(SeasonalCareerPathDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCareerPathDigestCodeLengthRule(int MaxLength) : ISeasonalCareerPathDigestRule
{
    public List<string> Check(SeasonalCareerPathDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCareerPathDigestDiscountRangeRule(double Min, double Max) : ISeasonalCareerPathDigestRule
{
    public List<string> Check(SeasonalCareerPathDigest item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal career path digest.</summary>
public sealed record SeasonalCareerPathDigestValidator(List<ISeasonalCareerPathDigestRule> Rules)
{
    public List<string> Validate(SeasonalCareerPathDigest item)
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

    public void RequireValid(SeasonalCareerPathDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalCareerPathDigest> items)
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

/// <summary>Functions of seasonal career path digest rules.</summary>
public static class SeasonalCareerPathDigestRules
{
    public static SeasonalCareerPathDigestValidator DefaultSeasonalCareerPathDigestValidator()
    {
        return new SeasonalCareerPathDigestValidator(
            new List<ISeasonalCareerPathDigestRule>
            {
                new SeasonalCareerPathDigestCodePrefixRule("X"),
                new SeasonalCareerPathDigestRequiredCodeRule("seasonal career path digest needs an entry"),
                new SeasonalCareerPathDigestCodeLengthRule(64),
                new SeasonalCareerPathDigestDiscountRangeRule(0.1, 4826.4),
            });
    }
}
