namespace Acme.Workforce.Contractors.Offboarding.Slot;

/// <summary>A rule a offboarding allocation must pass. Check returns its problems, or none.</summary>
public interface IOffboardingAllocationRule
{
    List<string> Check(OffboardingAllocation item);
}

public sealed record OffboardingAllocationRateRangeRule(double Min, double Max) : IOffboardingAllocationRule
{
    public List<string> Check(OffboardingAllocation item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingAllocationAmountRangeRule(double Min, double Max) : IOffboardingAllocationRule
{
    public List<string> Check(OffboardingAllocation item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingAllocationNameLengthRule(int MaxLength) : IOffboardingAllocationRule
{
    public List<string> Check(OffboardingAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingAllocationCodeLengthRule(int MaxLength) : IOffboardingAllocationRule
{
    public List<string> Check(OffboardingAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingAllocationSourceLengthRule(int MaxLength) : IOffboardingAllocationRule
{
    public List<string> Check(OffboardingAllocation item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding allocation.</summary>
public sealed record OffboardingAllocationValidator(List<IOffboardingAllocationRule> Rules)
{
    public List<string> Validate(OffboardingAllocation item)
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

    public void RequireValid(OffboardingAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingAllocation> items)
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

/// <summary>Functions of offboarding allocation rules.</summary>
public static class OffboardingAllocationRules
{
    public static OffboardingAllocationValidator DefaultOffboardingAllocationValidator()
    {
        return new OffboardingAllocationValidator(
            new List<IOffboardingAllocationRule>
            {
                new OffboardingAllocationRateRangeRule(9.4, 103.1),
                new OffboardingAllocationAmountRangeRule(9.3, 4836.6),
                new OffboardingAllocationNameLengthRule(3600),
                new OffboardingAllocationCodeLengthRule(86400),
                new OffboardingAllocationSourceLengthRule(3600),
            });
    }
}
