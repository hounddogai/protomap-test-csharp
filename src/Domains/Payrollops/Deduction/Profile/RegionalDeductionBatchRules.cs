namespace Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>A rule a regional deduction batch must pass. Check returns its problems, or none.</summary>
public interface IRegionalDeductionBatchRule
{
    List<string> Check(RegionalDeductionBatch item);
}

public sealed record RegionalDeductionBatchNameLengthRule(int MaxLength) : IRegionalDeductionBatchRule
{
    public List<string> Check(RegionalDeductionBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeductionBatchSourceLengthRule(int MaxLength) : IRegionalDeductionBatchRule
{
    public List<string> Check(RegionalDeductionBatch item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeductionBatchEndsAtRangeRule(int Min, int Max) : IRegionalDeductionBatchRule
{
    public List<string> Check(RegionalDeductionBatch item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional deduction batch.</summary>
public sealed record RegionalDeductionBatchValidator(List<IRegionalDeductionBatchRule> Rules)
{
    public List<string> Validate(RegionalDeductionBatch item)
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

    public void RequireValid(RegionalDeductionBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalDeductionBatch> items)
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

/// <summary>Functions of regional deduction batch rules.</summary>
public static class RegionalDeductionBatchRules
{
    public static RegionalDeductionBatchValidator DefaultRegionalDeductionBatchValidator()
    {
        return new RegionalDeductionBatchValidator(
            new List<IRegionalDeductionBatchRule>
            {
                new RegionalDeductionBatchNameLengthRule(80),
                new RegionalDeductionBatchSourceLengthRule(16),
                new RegionalDeductionBatchEndsAtRangeRule(32, 80),
            });
    }
}
