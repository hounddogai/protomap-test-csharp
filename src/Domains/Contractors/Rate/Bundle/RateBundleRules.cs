namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rate bundle must pass. Check returns its problems, or none.</summary>
public interface IRateBundleRule
{
    List<string> Check(RateBundle item);
}

public sealed record RateBundleRequiredLabelRule(string Message) : IRateBundleRule
{
    public List<string> Check(RateBundle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateBundleLabelLengthRule(int MaxLength) : IRateBundleRule
{
    public List<string> Check(RateBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateBundleRequiredUnitRule(string Message) : IRateBundleRule
{
    public List<string> Check(RateBundle item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateBundleUnitLengthRule(int MaxLength) : IRateBundleRule
{
    public List<string> Check(RateBundle item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateBundleAllowedUnitRule(List<string> Allowed) : IRateBundleRule
{
    public List<string> Check(RateBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record RateBundlePinnedGuardRule(double Limit) : IRateBundleRule
{
    public List<string> Check(RateBundle item)
    {
        if (item.Pinned && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate bundle.</summary>
public sealed record RateBundleValidator(List<IRateBundleRule> Rules)
{
    public List<string> Validate(RateBundle item)
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

    public void RequireValid(RateBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateBundle> items)
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

/// <summary>Functions of rate bundle rules.</summary>
public static class RateBundleRules
{
    public static RateBundleValidator DefaultRateBundleValidator()
    {
        return new RateBundleValidator(
            new List<IRateBundleRule>
            {
                new RateBundleRequiredLabelRule("rate bundle needs a value"),
                new RateBundleLabelLengthRule(64),
                new RateBundleRequiredUnitRule("rate bundle needs an entry"),
                new RateBundleUnitLengthRule(3600),
                new RateBundleAllowedUnitRule(new List<string> { "adaptive", "seasonal" }),
                new RateBundlePinnedGuardRule(2.6),
            });
    }
}
