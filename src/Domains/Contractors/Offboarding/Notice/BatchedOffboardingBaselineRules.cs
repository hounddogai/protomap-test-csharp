namespace Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>A rule a batched offboarding baseline must pass. Check returns its problems, or none.</summary>
public interface IBatchedOffboardingBaselineRule
{
    List<string> Check(BatchedOffboardingBaseline item);
}

public sealed record BatchedOffboardingBaselineDepartmentLengthRule(int MaxLength) : IBatchedOffboardingBaselineRule
{
    public List<string> Check(BatchedOffboardingBaseline item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedOffboardingBaselineFloorValueRangeRule(double Min, double Max) : IBatchedOffboardingBaselineRule
{
    public List<string> Check(BatchedOffboardingBaseline item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedOffboardingBaselineManagerIdLengthRule(int MaxLength) : IBatchedOffboardingBaselineRule
{
    public List<string> Check(BatchedOffboardingBaseline item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedOffboardingBaselineRequiredCodeRule(string Message) : IBatchedOffboardingBaselineRule
{
    public List<string> Check(BatchedOffboardingBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedOffboardingBaselineCodePrefixRule(string Prefix) : IBatchedOffboardingBaselineRule
{
    public List<string> Check(BatchedOffboardingBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedOffboardingBaselineRequiredManagerIdRule(string Message) : IBatchedOffboardingBaselineRule
{
    public List<string> Check(BatchedOffboardingBaseline item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched offboarding baseline.</summary>
public sealed record BatchedOffboardingBaselineValidator(List<IBatchedOffboardingBaselineRule> Rules)
{
    public List<string> Validate(BatchedOffboardingBaseline item)
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

    public void RequireValid(BatchedOffboardingBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedOffboardingBaseline> items)
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

/// <summary>Functions of batched offboarding baseline rules.</summary>
public static class BatchedOffboardingBaselineRules
{
    public static BatchedOffboardingBaselineValidator DefaultBatchedOffboardingBaselineValidator()
    {
        return new BatchedOffboardingBaselineValidator(
            new List<IBatchedOffboardingBaselineRule>
            {
                new BatchedOffboardingBaselineDepartmentLengthRule(16),
                new BatchedOffboardingBaselineFloorValueRangeRule(8.3, 150.3),
                new BatchedOffboardingBaselineManagerIdLengthRule(86400),
                new BatchedOffboardingBaselineRequiredCodeRule("batched offboarding baseline needs a value"),
                new BatchedOffboardingBaselineCodePrefixRule("OPS"),
                new BatchedOffboardingBaselineRequiredManagerIdRule("batched offboarding baseline needs an entry"),
            });
    }
}
