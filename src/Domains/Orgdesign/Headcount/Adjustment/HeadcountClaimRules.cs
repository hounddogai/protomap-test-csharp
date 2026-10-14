namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>A rule a headcount claim must pass. Check returns its problems, or none.</summary>
public interface IHeadcountClaimRule
{
    List<string> Check(HeadcountClaim item);
}

public sealed record HeadcountClaimChannelLengthRule(int MaxLength) : IHeadcountClaimRule
{
    public List<string> Check(HeadcountClaim item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountClaimExpiresAtRangeRule(int Min, int Max) : IHeadcountClaimRule
{
    public List<string> Check(HeadcountClaim item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountClaimRateRangeRule(double Min, double Max) : IHeadcountClaimRule
{
    public List<string> Check(HeadcountClaim item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountClaimCapacityRangeRule(int Min, int Max) : IHeadcountClaimRule
{
    public List<string> Check(HeadcountClaim item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountClaimTitleLengthRule(int MaxLength) : IHeadcountClaimRule
{
    public List<string> Check(HeadcountClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount claim.</summary>
public sealed record HeadcountClaimValidator(List<IHeadcountClaimRule> Rules)
{
    public List<string> Validate(HeadcountClaim item)
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

    public void RequireValid(HeadcountClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountClaim> items)
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

/// <summary>Functions of headcount claim rules.</summary>
public static class HeadcountClaimRules
{
    public static HeadcountClaimValidator DefaultHeadcountClaimValidator()
    {
        return new HeadcountClaimValidator(
            new List<IHeadcountClaimRule>
            {
                new HeadcountClaimChannelLengthRule(3600),
                new HeadcountClaimExpiresAtRangeRule(16, 86400),
                new HeadcountClaimRateRangeRule(1.0, 2811.6),
                new HeadcountClaimCapacityRangeRule(16, 3600),
                new HeadcountClaimTitleLengthRule(80),
            });
    }
}
