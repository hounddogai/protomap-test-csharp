namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

/// <summary>A rule a pending laptop milestone must pass. Check returns its problems, or none.</summary>
public interface IPendingLaptopMilestoneRule
{
    List<string> Check(PendingLaptopMilestone item);
}

public sealed record PendingLaptopMilestoneNameLengthRule(int MaxLength) : IPendingLaptopMilestoneRule
{
    public List<string> Check(PendingLaptopMilestone item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingLaptopMilestoneRequiredTitleRule(string Message) : IPendingLaptopMilestoneRule
{
    public List<string> Check(PendingLaptopMilestone item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingLaptopMilestoneExpiresAtRangeRule(int Min, int Max) : IPendingLaptopMilestoneRule
{
    public List<string> Check(PendingLaptopMilestone item)
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

public sealed record PendingLaptopMilestoneCodePrefixRule(string Prefix) : IPendingLaptopMilestoneRule
{
    public List<string> Check(PendingLaptopMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingLaptopMilestoneWindowDaysRangeRule(int Min, int Max) : IPendingLaptopMilestoneRule
{
    public List<string> Check(PendingLaptopMilestone item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingLaptopMilestoneTitleLengthRule(int MaxLength) : IPendingLaptopMilestoneRule
{
    public List<string> Check(PendingLaptopMilestone item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending laptop milestone.</summary>
public sealed record PendingLaptopMilestoneValidator(List<IPendingLaptopMilestoneRule> Rules)
{
    public List<string> Validate(PendingLaptopMilestone item)
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

    public void RequireValid(PendingLaptopMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingLaptopMilestone> items)
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

/// <summary>Functions of pending laptop milestone rules.</summary>
public static class PendingLaptopMilestoneRules
{
    public static PendingLaptopMilestoneValidator DefaultPendingLaptopMilestoneValidator()
    {
        return new PendingLaptopMilestoneValidator(
            new List<IPendingLaptopMilestoneRule>
            {
                new PendingLaptopMilestoneNameLengthRule(32),
                new PendingLaptopMilestoneRequiredTitleRule("pending laptop milestone needs a value"),
                new PendingLaptopMilestoneExpiresAtRangeRule(16, 16),
                new PendingLaptopMilestoneCodePrefixRule("REF"),
                new PendingLaptopMilestoneWindowDaysRangeRule(64, 64),
                new PendingLaptopMilestoneTitleLengthRule(86400),
            });
    }
}
