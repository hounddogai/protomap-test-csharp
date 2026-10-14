namespace Acme.Workforce.Contractors.Rate.RateBatch;

/// <summary>A rule a rate milestone must pass. Check returns its problems, or none.</summary>
public interface IRateMilestoneRule
{
    List<string> Check(RateMilestone item);
}

public sealed record RateMilestoneVersionRangeRule(int Min, int Max) : IRateMilestoneRule
{
    public List<string> Check(RateMilestone item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RateMilestoneRequiredTimezoneRule(string Message) : IRateMilestoneRule
{
    public List<string> Check(RateMilestone item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateMilestoneCodeLengthRule(int MaxLength) : IRateMilestoneRule
{
    public List<string> Check(RateMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateMilestoneNameLengthRule(int MaxLength) : IRateMilestoneRule
{
    public List<string> Check(RateMilestone item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateMilestoneTimezoneLengthRule(int MaxLength) : IRateMilestoneRule
{
    public List<string> Check(RateMilestone item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate milestone.</summary>
public sealed record RateMilestoneValidator(List<IRateMilestoneRule> Rules)
{
    public List<string> Validate(RateMilestone item)
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

    public void RequireValid(RateMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateMilestone> items)
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

/// <summary>Functions of rate milestone rules.</summary>
public static class RateMilestoneRules
{
    public static RateMilestoneValidator DefaultRateMilestoneValidator()
    {
        return new RateMilestoneValidator(
            new List<IRateMilestoneRule>
            {
                new RateMilestoneVersionRangeRule(80, 80),
                new RateMilestoneRequiredTimezoneRule("rate milestone needs an entry"),
                new RateMilestoneCodeLengthRule(32),
                new RateMilestoneNameLengthRule(32),
                new RateMilestoneTimezoneLengthRule(1000),
            });
    }
}
