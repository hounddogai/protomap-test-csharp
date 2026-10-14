namespace Acme.Workforce.Workplace.Visitor.Review;

/// <summary>A rule a adaptive visitor checkpoint must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveVisitorCheckpointRule
{
    List<string> Check(AdaptiveVisitorCheckpoint item);
}

public sealed record AdaptiveVisitorCheckpointCodePrefixRule(string Prefix) : IAdaptiveVisitorCheckpointRule
{
    public List<string> Check(AdaptiveVisitorCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveVisitorCheckpointRequiredTitleRule(string Message) : IAdaptiveVisitorCheckpointRule
{
    public List<string> Check(AdaptiveVisitorCheckpoint item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveVisitorCheckpointRequiredCountryRule(string Message) : IAdaptiveVisitorCheckpointRule
{
    public List<string> Check(AdaptiveVisitorCheckpoint item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveVisitorCheckpointTitleLengthRule(int MaxLength) : IAdaptiveVisitorCheckpointRule
{
    public List<string> Check(AdaptiveVisitorCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveVisitorCheckpointGraceDaysRangeRule(int Min, int Max) : IAdaptiveVisitorCheckpointRule
{
    public List<string> Check(AdaptiveVisitorCheckpoint item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveVisitorCheckpointCountryLengthRule(int MaxLength) : IAdaptiveVisitorCheckpointRule
{
    public List<string> Check(AdaptiveVisitorCheckpoint item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive visitor checkpoint.</summary>
public sealed record AdaptiveVisitorCheckpointValidator(List<IAdaptiveVisitorCheckpointRule> Rules)
{
    public List<string> Validate(AdaptiveVisitorCheckpoint item)
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

    public void RequireValid(AdaptiveVisitorCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveVisitorCheckpoint> items)
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

/// <summary>Functions of adaptive visitor checkpoint rules.</summary>
public static class AdaptiveVisitorCheckpointRules
{
    public static AdaptiveVisitorCheckpointValidator DefaultAdaptiveVisitorCheckpointValidator()
    {
        return new AdaptiveVisitorCheckpointValidator(
            new List<IAdaptiveVisitorCheckpointRule>
            {
                new AdaptiveVisitorCheckpointCodePrefixRule("REF"),
                new AdaptiveVisitorCheckpointRequiredTitleRule("adaptive visitor checkpoint needs this field"),
                new AdaptiveVisitorCheckpointRequiredCountryRule("adaptive visitor checkpoint needs an entry"),
                new AdaptiveVisitorCheckpointTitleLengthRule(64),
                new AdaptiveVisitorCheckpointGraceDaysRangeRule(64, 255),
                new AdaptiveVisitorCheckpointCountryLengthRule(80),
            });
    }
}
