namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a refresh allocation must pass. Check returns its problems, or none.</summary>
public interface IRefreshAllocationRule
{
    List<string> Check(RefreshAllocation item);
}

public sealed record RefreshAllocationActiveGuardRule(int Limit) : IRefreshAllocationRule
{
    public List<string> Check(RefreshAllocation item)
    {
        if (item.Active && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record RefreshAllocationStartsAtRangeRule(int Min, int Max) : IRefreshAllocationRule
{
    public List<string> Check(RefreshAllocation item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RefreshAllocationRequiredCodeRule(string Message) : IRefreshAllocationRule
{
    public List<string> Check(RefreshAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshAllocationCodeLengthRule(int MaxLength) : IRefreshAllocationRule
{
    public List<string> Check(RefreshAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshAllocationAllowedCodeRule(List<string> Allowed) : IRefreshAllocationRule
{
    public List<string> Check(RefreshAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record RefreshAllocationNameLengthRule(int MaxLength) : IRefreshAllocationRule
{
    public List<string> Check(RefreshAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh allocation.</summary>
public sealed record RefreshAllocationValidator(List<IRefreshAllocationRule> Rules)
{
    public List<string> Validate(RefreshAllocation item)
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

    public void RequireValid(RefreshAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshAllocation> items)
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

/// <summary>Functions of refresh allocation rules.</summary>
public static class RefreshAllocationRules
{
    public static RefreshAllocationValidator DefaultRefreshAllocationValidator()
    {
        return new RefreshAllocationValidator(
            new List<IRefreshAllocationRule>
            {
                new RefreshAllocationActiveGuardRule(86400),
                new RefreshAllocationStartsAtRangeRule(80, 86400),
                new RefreshAllocationRequiredCodeRule("refresh allocation needs a value"),
                new RefreshAllocationCodeLengthRule(255),
                new RefreshAllocationAllowedCodeRule(new List<string> { "weighted", "scheduled", "rolling" }),
                new RefreshAllocationNameLengthRule(255),
            });
    }
}
