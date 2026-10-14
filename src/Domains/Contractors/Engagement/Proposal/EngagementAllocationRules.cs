namespace Acme.Workforce.Contractors.Engagement.Proposal;

/// <summary>A rule a engagement allocation must pass. Check returns its problems, or none.</summary>
public interface IEngagementAllocationRule
{
    List<string> Check(EngagementAllocation item);
}

public sealed record EngagementAllocationNotesLengthRule(int MaxLength) : IEngagementAllocationRule
{
    public List<string> Check(EngagementAllocation item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementAllocationCodeLengthRule(int MaxLength) : IEngagementAllocationRule
{
    public List<string> Check(EngagementAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementAllocationUpdatedAtRangeRule(int Min, int Max) : IEngagementAllocationRule
{
    public List<string> Check(EngagementAllocation item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementAllocationLabelLengthRule(int MaxLength) : IEngagementAllocationRule
{
    public List<string> Check(EngagementAllocation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementAllocationRequiredLabelRule(string Message) : IEngagementAllocationRule
{
    public List<string> Check(EngagementAllocation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementAllocationRequiredCodeRule(string Message) : IEngagementAllocationRule
{
    public List<string> Check(EngagementAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement allocation.</summary>
public sealed record EngagementAllocationValidator(List<IEngagementAllocationRule> Rules)
{
    public List<string> Validate(EngagementAllocation item)
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

    public void RequireValid(EngagementAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementAllocation> items)
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

/// <summary>Functions of engagement allocation rules.</summary>
public static class EngagementAllocationRules
{
    public static EngagementAllocationValidator DefaultEngagementAllocationValidator()
    {
        return new EngagementAllocationValidator(
            new List<IEngagementAllocationRule>
            {
                new EngagementAllocationNotesLengthRule(3600),
                new EngagementAllocationCodeLengthRule(3600),
                new EngagementAllocationUpdatedAtRangeRule(80, 86400),
                new EngagementAllocationLabelLengthRule(64),
                new EngagementAllocationRequiredLabelRule("engagement allocation needs a value"),
                new EngagementAllocationRequiredCodeRule("engagement allocation needs this field"),
            });
    }
}
