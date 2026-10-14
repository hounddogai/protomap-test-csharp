namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adjustment allocation must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentAllocationRule
{
    List<string> Check(AdjustmentAllocation item);
}

public sealed record AdjustmentAllocationCurrencyLengthRule(int MaxLength) : IAdjustmentAllocationRule
{
    public List<string> Check(AdjustmentAllocation item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentAllocationAllowedNameRule(List<string> Allowed) : IAdjustmentAllocationRule
{
    public List<string> Check(AdjustmentAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentAllocationRequiredTimezoneRule(string Message) : IAdjustmentAllocationRule
{
    public List<string> Check(AdjustmentAllocation item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentAllocationRequiredCurrencyRule(string Message) : IAdjustmentAllocationRule
{
    public List<string> Check(AdjustmentAllocation item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment allocation.</summary>
public sealed record AdjustmentAllocationValidator(List<IAdjustmentAllocationRule> Rules)
{
    public List<string> Validate(AdjustmentAllocation item)
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

    public void RequireValid(AdjustmentAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentAllocation> items)
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

/// <summary>Functions of adjustment allocation rules.</summary>
public static class AdjustmentAllocationRules
{
    public static AdjustmentAllocationValidator DefaultAdjustmentAllocationValidator()
    {
        return new AdjustmentAllocationValidator(
            new List<IAdjustmentAllocationRule>
            {
                new AdjustmentAllocationCurrencyLengthRule(64),
                new AdjustmentAllocationAllowedNameRule(new List<string> { "regional" }),
                new AdjustmentAllocationRequiredTimezoneRule("adjustment allocation needs this field"),
                new AdjustmentAllocationRequiredCurrencyRule("adjustment allocation needs a value"),
            });
    }
}
