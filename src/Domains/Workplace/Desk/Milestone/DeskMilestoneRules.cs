namespace Acme.Workforce.Workplace.Desk.Milestone;

/// <summary>A rule a desk milestone must pass. Check returns its problems, or none.</summary>
public interface IDeskMilestoneRule
{
    List<string> Check(DeskMilestone item);
}

public sealed record DeskMilestoneLeaveDaysRangeRule(int Min, int Max) : IDeskMilestoneRule
{
    public List<string> Check(DeskMilestone item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskMilestoneCodePrefixRule(string Prefix) : IDeskMilestoneRule
{
    public List<string> Check(DeskMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskMilestoneNameLengthRule(int MaxLength) : IDeskMilestoneRule
{
    public List<string> Check(DeskMilestone item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk milestone.</summary>
public sealed record DeskMilestoneValidator(List<IDeskMilestoneRule> Rules)
{
    public List<string> Validate(DeskMilestone item)
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

    public void RequireValid(DeskMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskMilestone> items)
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

/// <summary>Functions of desk milestone rules.</summary>
public static class DeskMilestoneRules
{
    public static DeskMilestoneValidator DefaultDeskMilestoneValidator()
    {
        return new DeskMilestoneValidator(
            new List<IDeskMilestoneRule>
            {
                new DeskMilestoneLeaveDaysRangeRule(64, 86400),
                new DeskMilestoneCodePrefixRule("REF"),
                new DeskMilestoneNameLengthRule(16),
            });
    }
}
