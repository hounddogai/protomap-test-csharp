namespace Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>A rule a global pay band allocation must pass. Check returns its problems, or none.</summary>
public interface IGlobalPayBandAllocationRule
{
    List<string> Check(GlobalPayBandAllocation item);
}

public sealed record GlobalPayBandAllocationVisibleGuardRule(double Limit) : IGlobalPayBandAllocationRule
{
    public List<string> Check(GlobalPayBandAllocation item)
    {
        if (item.Visible && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPayBandAllocationRequiredChannelRule(string Message) : IGlobalPayBandAllocationRule
{
    public List<string> Check(GlobalPayBandAllocation item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalPayBandAllocationTagsLimitRule(int MaxCount, List<string> Banned) : IGlobalPayBandAllocationRule
{
    public List<string> Check(GlobalPayBandAllocation item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

/// <summary>Runs every rule of a global pay band allocation.</summary>
public sealed record GlobalPayBandAllocationValidator(List<IGlobalPayBandAllocationRule> Rules)
{
    public List<string> Validate(GlobalPayBandAllocation item)
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

    public void RequireValid(GlobalPayBandAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPayBandAllocation> items)
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

/// <summary>Functions of global pay band allocation rules.</summary>
public static class GlobalPayBandAllocationRules
{
    public static GlobalPayBandAllocationValidator DefaultGlobalPayBandAllocationValidator()
    {
        return new GlobalPayBandAllocationValidator(
            new List<IGlobalPayBandAllocationRule>
            {
                new GlobalPayBandAllocationVisibleGuardRule(0.0),
                new GlobalPayBandAllocationRequiredChannelRule("global pay band allocation needs a value"),
                new GlobalPayBandAllocationTagsLimitRule(32, new List<string> { "manual", "manual", "adaptive" }),
            });
    }
}
