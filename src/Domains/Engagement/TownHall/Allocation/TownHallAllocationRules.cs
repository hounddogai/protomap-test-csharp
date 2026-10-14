namespace Acme.Workforce.Engagement.TownHall.Allocation;

/// <summary>A rule a town hall allocation must pass. Check returns its problems, or none.</summary>
public interface ITownHallAllocationRule
{
    List<string> Check(TownHallAllocation item);
}

public sealed record TownHallAllocationGradeLengthRule(int MaxLength) : ITownHallAllocationRule
{
    public List<string> Check(TownHallAllocation item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallAllocationRequiredGradeRule(string Message) : ITownHallAllocationRule
{
    public List<string> Check(TownHallAllocation item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallAllocationCodeLengthRule(int MaxLength) : ITownHallAllocationRule
{
    public List<string> Check(TownHallAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallAllocationCodePrefixRule(string Prefix) : ITownHallAllocationRule
{
    public List<string> Check(TownHallAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallAllocationRequiredCodeRule(string Message) : ITownHallAllocationRule
{
    public List<string> Check(TownHallAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallAllocationNameLengthRule(int MaxLength) : ITownHallAllocationRule
{
    public List<string> Check(TownHallAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall allocation.</summary>
public sealed record TownHallAllocationValidator(List<ITownHallAllocationRule> Rules)
{
    public List<string> Validate(TownHallAllocation item)
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

    public void RequireValid(TownHallAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallAllocation> items)
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

/// <summary>Functions of town hall allocation rules.</summary>
public static class TownHallAllocationRules
{
    public static TownHallAllocationValidator DefaultTownHallAllocationValidator()
    {
        return new TownHallAllocationValidator(
            new List<ITownHallAllocationRule>
            {
                new TownHallAllocationGradeLengthRule(3600),
                new TownHallAllocationRequiredGradeRule("town hall allocation needs a value"),
                new TownHallAllocationCodeLengthRule(80),
                new TownHallAllocationCodePrefixRule("Q"),
                new TownHallAllocationRequiredCodeRule("town hall allocation needs this field"),
                new TownHallAllocationNameLengthRule(1000),
            });
    }
}
