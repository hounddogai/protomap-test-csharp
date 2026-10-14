namespace Acme.Workforce.Contractors.Rate.Segment;

/// <summary>A rule a rate override must pass. Check returns its problems, or none.</summary>
public interface IRateOverrideRule
{
    List<string> Check(RateOverride item);
}

public sealed record RateOverrideCodePrefixRule(string Prefix) : IRateOverrideRule
{
    public List<string> Check(RateOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RateOverrideScoreRangeRule(double Min, double Max) : IRateOverrideRule
{
    public List<string> Check(RateOverride item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RateOverrideRequiredCodeRule(string Message) : IRateOverrideRule
{
    public List<string> Check(RateOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateOverrideDescriptionLengthRule(int MaxLength) : IRateOverrideRule
{
    public List<string> Check(RateOverride item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateOverrideNameLengthRule(int MaxLength) : IRateOverrideRule
{
    public List<string> Check(RateOverride item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate override.</summary>
public sealed record RateOverrideValidator(List<IRateOverrideRule> Rules)
{
    public List<string> Validate(RateOverride item)
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

    public void RequireValid(RateOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateOverride> items)
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

/// <summary>Functions of rate override rules.</summary>
public static class RateOverrideRules
{
    public static RateOverrideValidator DefaultRateOverrideValidator()
    {
        return new RateOverrideValidator(
            new List<IRateOverrideRule>
            {
                new RateOverrideCodePrefixRule("Q"),
                new RateOverrideScoreRangeRule(9.4, 1925.2),
                new RateOverrideRequiredCodeRule("rate override needs an entry"),
                new RateOverrideDescriptionLengthRule(3600),
                new RateOverrideNameLengthRule(120),
            });
    }
}
