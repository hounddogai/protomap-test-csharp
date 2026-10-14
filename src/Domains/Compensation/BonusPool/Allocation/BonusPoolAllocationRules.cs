namespace Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>A rule a bonus pool allocation must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolAllocationRule
{
    List<string> Check(BonusPoolAllocation item);
}

public sealed record BonusPoolAllocationRequiredPhoneRule(string Message) : IBonusPoolAllocationRule
{
    public List<string> Check(BonusPoolAllocation item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolAllocationCodeLengthRule(int MaxLength) : IBonusPoolAllocationRule
{
    public List<string> Check(BonusPoolAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolAllocationNameLengthRule(int MaxLength) : IBonusPoolAllocationRule
{
    public List<string> Check(BonusPoolAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolAllocationRequiredCodeRule(string Message) : IBonusPoolAllocationRule
{
    public List<string> Check(BonusPoolAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolAllocationAttemptsRangeRule(int Min, int Max) : IBonusPoolAllocationRule
{
    public List<string> Check(BonusPoolAllocation item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolAllocationRequiredNameRule(string Message) : IBonusPoolAllocationRule
{
    public List<string> Check(BonusPoolAllocation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool allocation.</summary>
public sealed record BonusPoolAllocationValidator(List<IBonusPoolAllocationRule> Rules)
{
    public List<string> Validate(BonusPoolAllocation item)
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

    public void RequireValid(BonusPoolAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolAllocation> items)
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

/// <summary>Functions of bonus pool allocation rules.</summary>
public static class BonusPoolAllocationRules
{
    public static BonusPoolAllocationValidator DefaultBonusPoolAllocationValidator()
    {
        return new BonusPoolAllocationValidator(
            new List<IBonusPoolAllocationRule>
            {
                new BonusPoolAllocationRequiredPhoneRule("bonus pool allocation needs a value"),
                new BonusPoolAllocationCodeLengthRule(64),
                new BonusPoolAllocationNameLengthRule(1000),
                new BonusPoolAllocationRequiredCodeRule("bonus pool allocation needs this field"),
                new BonusPoolAllocationAttemptsRangeRule(16, 16),
                new BonusPoolAllocationRequiredNameRule("bonus pool allocation needs this field"),
            });
    }
}
