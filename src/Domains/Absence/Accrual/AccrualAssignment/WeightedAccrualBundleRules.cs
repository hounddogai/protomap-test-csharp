namespace Acme.Workforce.Absence.Accrual.AccrualAssignment;

/// <summary>A rule a weighted accrual bundle must pass. Check returns its problems, or none.</summary>
public interface IWeightedAccrualBundleRule
{
    List<string> Check(WeightedAccrualBundle item);
}

public sealed record WeightedAccrualBundleCodeLengthRule(int MaxLength) : IWeightedAccrualBundleRule
{
    public List<string> Check(WeightedAccrualBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccrualBundleRequiredCodeRule(string Message) : IWeightedAccrualBundleRule
{
    public List<string> Check(WeightedAccrualBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccrualBundleNameLengthRule(int MaxLength) : IWeightedAccrualBundleRule
{
    public List<string> Check(WeightedAccrualBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccrualBundleCodePrefixRule(string Prefix) : IWeightedAccrualBundleRule
{
    public List<string> Check(WeightedAccrualBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccrualBundleCapacityRangeRule(int Min, int Max) : IWeightedAccrualBundleRule
{
    public List<string> Check(WeightedAccrualBundle item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a weighted accrual bundle.</summary>
public sealed record WeightedAccrualBundleValidator(List<IWeightedAccrualBundleRule> Rules)
{
    public List<string> Validate(WeightedAccrualBundle item)
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

    public void RequireValid(WeightedAccrualBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAccrualBundle> items)
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

/// <summary>Functions of weighted accrual bundle rules.</summary>
public static class WeightedAccrualBundleRules
{
    public static WeightedAccrualBundleValidator DefaultWeightedAccrualBundleValidator()
    {
        return new WeightedAccrualBundleValidator(
            new List<IWeightedAccrualBundleRule>
            {
                new WeightedAccrualBundleCodeLengthRule(1000),
                new WeightedAccrualBundleRequiredCodeRule("weighted accrual bundle needs an entry"),
                new WeightedAccrualBundleNameLengthRule(3600),
                new WeightedAccrualBundleCodePrefixRule("Q"),
                new WeightedAccrualBundleCapacityRangeRule(1000, 1000),
            });
    }
}
