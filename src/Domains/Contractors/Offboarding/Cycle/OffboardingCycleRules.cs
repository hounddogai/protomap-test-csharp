namespace Acme.Workforce.Contractors.Offboarding.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding cycle must pass. Check returns its problems, or none.</summary>
public interface IOffboardingCycleRule
{
    List<string> Check(OffboardingCycle item);
}

public sealed record OffboardingCycleRequiredNameRule(string Message) : IOffboardingCycleRule
{
    public List<string> Check(OffboardingCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingCycleAllowedCostCenterRule(List<string> Allowed) : IOffboardingCycleRule
{
    public List<string> Check(OffboardingCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is missing" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingCycleCostCenterLengthRule(int MaxLength) : IOffboardingCycleRule
{
    public List<string> Check(OffboardingCycle item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingCycleNameLengthRule(int MaxLength) : IOffboardingCycleRule
{
    public List<string> Check(OffboardingCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingCycleLimitRangeRule(int Min, int Max) : IOffboardingCycleRule
{
    public List<string> Check(OffboardingCycle item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingCycleDepartmentLengthRule(int MaxLength) : IOffboardingCycleRule
{
    public List<string> Check(OffboardingCycle item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding cycle.</summary>
public sealed record OffboardingCycleValidator(List<IOffboardingCycleRule> Rules)
{
    public List<string> Validate(OffboardingCycle item)
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

    public void RequireValid(OffboardingCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingCycle> items)
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

/// <summary>Functions of offboarding cycle rules.</summary>
public static class OffboardingCycleRules
{
    public static OffboardingCycleValidator DefaultOffboardingCycleValidator()
    {
        return new OffboardingCycleValidator(
            new List<IOffboardingCycleRule>
            {
                new OffboardingCycleRequiredNameRule("offboarding cycle needs this field"),
                new OffboardingCycleAllowedCostCenterRule(new List<string> { "batched" }),
                new OffboardingCycleCostCenterLengthRule(64),
                new OffboardingCycleNameLengthRule(255),
                new OffboardingCycleLimitRangeRule(64, 86400),
                new OffboardingCycleDepartmentLengthRule(120),
            });
    }
}
