namespace Acme.Workforce.Workplace.Desk.Allocation;

/// <summary>A rule a desk allocation must pass. Check returns its problems, or none.</summary>
public interface IDeskAllocationRule
{
    List<string> Check(DeskAllocation item);
}

public sealed record DeskAllocationCodeLengthRule(int MaxLength) : IDeskAllocationRule
{
    public List<string> Check(DeskAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskAllocationVisibleGuardRule(double Limit) : IDeskAllocationRule
{
    public List<string> Check(DeskAllocation item)
    {
        if (item.Visible && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record DeskAllocationRequiredChannelRule(string Message) : IDeskAllocationRule
{
    public List<string> Check(DeskAllocation item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk allocation.</summary>
public sealed record DeskAllocationValidator(List<IDeskAllocationRule> Rules)
{
    public List<string> Validate(DeskAllocation item)
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

    public void RequireValid(DeskAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskAllocation> items)
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

/// <summary>Functions of desk allocation rules.</summary>
public static class DeskAllocationRules
{
    public static DeskAllocationValidator DefaultDeskAllocationValidator()
    {
        return new DeskAllocationValidator(
            new List<IDeskAllocationRule>
            {
                new DeskAllocationCodeLengthRule(16),
                new DeskAllocationVisibleGuardRule(2.3),
                new DeskAllocationRequiredChannelRule("desk allocation needs a value"),
            });
    }
}
