namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a merit cycle allocation must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleAllocationRule
{
    List<string> Check(MeritCycleAllocation item);
}

public sealed record MeritCycleAllocationTitleLengthRule(int MaxLength) : IMeritCycleAllocationRule
{
    public List<string> Check(MeritCycleAllocation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleAllocationCostRangeRule(double Min, double Max) : IMeritCycleAllocationRule
{
    public List<string> Check(MeritCycleAllocation item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleAllocationRequiredTitleRule(string Message) : IMeritCycleAllocationRule
{
    public List<string> Check(MeritCycleAllocation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleAllocationTagsLimitRule(int MaxCount, List<string> Banned) : IMeritCycleAllocationRule
{
    public List<string> Check(MeritCycleAllocation item)
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

public sealed record MeritCycleAllocationAllowedCountryRule(List<string> Allowed) : IMeritCycleAllocationRule
{
    public List<string> Check(MeritCycleAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleAllocationWindowDaysRangeRule(int Min, int Max) : IMeritCycleAllocationRule
{
    public List<string> Check(MeritCycleAllocation item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle allocation.</summary>
public sealed record MeritCycleAllocationValidator(List<IMeritCycleAllocationRule> Rules)
{
    public List<string> Validate(MeritCycleAllocation item)
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

    public void RequireValid(MeritCycleAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleAllocation> items)
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

/// <summary>Functions of merit cycle allocation rules.</summary>
public static class MeritCycleAllocationRules
{
    public static MeritCycleAllocationValidator DefaultMeritCycleAllocationValidator()
    {
        return new MeritCycleAllocationValidator(
            new List<IMeritCycleAllocationRule>
            {
                new MeritCycleAllocationTitleLengthRule(64),
                new MeritCycleAllocationCostRangeRule(1.3, 3274.5),
                new MeritCycleAllocationRequiredTitleRule("merit cycle allocation needs this field"),
                new MeritCycleAllocationTagsLimitRule(255, new List<string> { "capped", "legacy" }),
                new MeritCycleAllocationAllowedCountryRule(new List<string> { "default" }),
                new MeritCycleAllocationWindowDaysRangeRule(16, 3600),
            });
    }
}
