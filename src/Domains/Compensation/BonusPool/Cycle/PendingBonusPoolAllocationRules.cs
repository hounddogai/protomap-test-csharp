namespace Acme.Workforce.Compensation.BonusPool.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending bonus pool allocation must pass. Check returns its problems, or none.</summary>
public interface IPendingBonusPoolAllocationRule
{
    List<string> Check(PendingBonusPoolAllocation item);
}

public sealed record PendingBonusPoolAllocationRequiredCodeRule(string Message) : IPendingBonusPoolAllocationRule
{
    public List<string> Check(PendingBonusPoolAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingBonusPoolAllocationNameLengthRule(int MaxLength) : IPendingBonusPoolAllocationRule
{
    public List<string> Check(PendingBonusPoolAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBonusPoolAllocationDurationMinutesRangeRule(int Min, int Max) : IPendingBonusPoolAllocationRule
{
    public List<string> Check(PendingBonusPoolAllocation item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBonusPoolAllocationExpiresAtRangeRule(int Min, int Max) : IPendingBonusPoolAllocationRule
{
    public List<string> Check(PendingBonusPoolAllocation item)
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

public sealed record PendingBonusPoolAllocationAllowedSourceRule(List<string> Allowed) : IPendingBonusPoolAllocationRule
{
    public List<string> Check(PendingBonusPoolAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending bonus pool allocation.</summary>
public sealed record PendingBonusPoolAllocationValidator(List<IPendingBonusPoolAllocationRule> Rules)
{
    public List<string> Validate(PendingBonusPoolAllocation item)
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

    public void RequireValid(PendingBonusPoolAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingBonusPoolAllocation> items)
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

/// <summary>Functions of pending bonus pool allocation rules.</summary>
public static class PendingBonusPoolAllocationRules
{
    public static PendingBonusPoolAllocationValidator DefaultPendingBonusPoolAllocationValidator()
    {
        return new PendingBonusPoolAllocationValidator(
            new List<IPendingBonusPoolAllocationRule>
            {
                new PendingBonusPoolAllocationRequiredCodeRule("pending bonus pool allocation needs this field"),
                new PendingBonusPoolAllocationNameLengthRule(64),
                new PendingBonusPoolAllocationDurationMinutesRangeRule(120, 86400),
                new PendingBonusPoolAllocationExpiresAtRangeRule(80, 86400),
                new PendingBonusPoolAllocationAllowedSourceRule(new List<string> { "regional" }),
            });
    }
}
