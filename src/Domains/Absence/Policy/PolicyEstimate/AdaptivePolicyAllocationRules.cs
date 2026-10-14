namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>A rule a adaptive policy allocation must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePolicyAllocationRule
{
    List<string> Check(AdaptivePolicyAllocation item);
}

public sealed record AdaptivePolicyAllocationQuantityRangeRule(int Min, int Max) : IAdaptivePolicyAllocationRule
{
    public List<string> Check(AdaptivePolicyAllocation item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptivePolicyAllocationWeightRangeRule(double Min, double Max) : IAdaptivePolicyAllocationRule
{
    public List<string> Check(AdaptivePolicyAllocation item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyAllocationUnitLengthRule(int MaxLength) : IAdaptivePolicyAllocationRule
{
    public List<string> Check(AdaptivePolicyAllocation item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyAllocationCodeLengthRule(int MaxLength) : IAdaptivePolicyAllocationRule
{
    public List<string> Check(AdaptivePolicyAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyAllocationRequiredCodeRule(string Message) : IAdaptivePolicyAllocationRule
{
    public List<string> Check(AdaptivePolicyAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive policy allocation.</summary>
public sealed record AdaptivePolicyAllocationValidator(List<IAdaptivePolicyAllocationRule> Rules)
{
    public List<string> Validate(AdaptivePolicyAllocation item)
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

    public void RequireValid(AdaptivePolicyAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePolicyAllocation> items)
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

/// <summary>Functions of adaptive policy allocation rules.</summary>
public static class AdaptivePolicyAllocationRules
{
    public static AdaptivePolicyAllocationValidator DefaultAdaptivePolicyAllocationValidator()
    {
        return new AdaptivePolicyAllocationValidator(
            new List<IAdaptivePolicyAllocationRule>
            {
                new AdaptivePolicyAllocationQuantityRangeRule(120, 86400),
                new AdaptivePolicyAllocationWeightRangeRule(0.1, 3499.8),
                new AdaptivePolicyAllocationUnitLengthRule(32),
                new AdaptivePolicyAllocationCodeLengthRule(16),
                new AdaptivePolicyAllocationRequiredCodeRule("adaptive policy allocation needs an entry"),
            });
    }
}
