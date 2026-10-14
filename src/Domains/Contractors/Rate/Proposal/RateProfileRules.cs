namespace Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>A rule a rate profile must pass. Check returns its problems, or none.</summary>
public interface IRateProfileRule
{
    List<string> Check(RateProfile item);
}

public sealed record RateProfileCodePrefixRule(string Prefix) : IRateProfileRule
{
    public List<string> Check(RateProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RateProfileScoreRangeRule(double Min, double Max) : IRateProfileRule
{
    public List<string> Check(RateProfile item)
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

public sealed record RateProfileApprovedGuardRule(double Limit) : IRateProfileRule
{
    public List<string> Check(RateProfile item)
    {
        if (item.Approved && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record RateProfileTitleLengthRule(int MaxLength) : IRateProfileRule
{
    public List<string> Check(RateProfile item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate profile.</summary>
public sealed record RateProfileValidator(List<IRateProfileRule> Rules)
{
    public List<string> Validate(RateProfile item)
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

    public void RequireValid(RateProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateProfile> items)
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

/// <summary>Functions of rate profile rules.</summary>
public static class RateProfileRules
{
    public static RateProfileValidator DefaultRateProfileValidator()
    {
        return new RateProfileValidator(
            new List<IRateProfileRule>
            {
                new RateProfileCodePrefixRule("ACM"),
                new RateProfileScoreRangeRule(5.0, 216.9),
                new RateProfileApprovedGuardRule(1.0),
                new RateProfileTitleLengthRule(120),
            });
    }
}
