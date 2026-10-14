namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>A rule a pending desk allocation must pass. Check returns its problems, or none.</summary>
public interface IPendingDeskAllocationRule
{
    List<string> Check(PendingDeskAllocation item);
}

public sealed record PendingDeskAllocationRetriesRangeRule(int Min, int Max) : IPendingDeskAllocationRule
{
    public List<string> Check(PendingDeskAllocation item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskAllocationBadgeLengthRule(int MaxLength) : IPendingDeskAllocationRule
{
    public List<string> Check(PendingDeskAllocation item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskAllocationEndsAtRangeRule(int Min, int Max) : IPendingDeskAllocationRule
{
    public List<string> Check(PendingDeskAllocation item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingDeskAllocationLabelLengthRule(int MaxLength) : IPendingDeskAllocationRule
{
    public List<string> Check(PendingDeskAllocation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskAllocationRequiredTeamRule(string Message) : IPendingDeskAllocationRule
{
    public List<string> Check(PendingDeskAllocation item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskAllocationCodeLengthRule(int MaxLength) : IPendingDeskAllocationRule
{
    public List<string> Check(PendingDeskAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending desk allocation.</summary>
public sealed record PendingDeskAllocationValidator(List<IPendingDeskAllocationRule> Rules)
{
    public List<string> Validate(PendingDeskAllocation item)
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

    public void RequireValid(PendingDeskAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingDeskAllocation> items)
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

/// <summary>Functions of pending desk allocation rules.</summary>
public static class PendingDeskAllocationRules
{
    public static PendingDeskAllocationValidator DefaultPendingDeskAllocationValidator()
    {
        return new PendingDeskAllocationValidator(
            new List<IPendingDeskAllocationRule>
            {
                new PendingDeskAllocationRetriesRangeRule(80, 3600),
                new PendingDeskAllocationBadgeLengthRule(255),
                new PendingDeskAllocationEndsAtRangeRule(16, 1000),
                new PendingDeskAllocationLabelLengthRule(3600),
                new PendingDeskAllocationRequiredTeamRule("pending desk allocation needs this field"),
                new PendingDeskAllocationCodeLengthRule(32),
            });
    }
}
