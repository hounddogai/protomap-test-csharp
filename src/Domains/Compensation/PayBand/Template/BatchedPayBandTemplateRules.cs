namespace Acme.Workforce.Compensation.PayBand.Template;

/// <summary>A rule a batched pay band template must pass. Check returns its problems, or none.</summary>
public interface IBatchedPayBandTemplateRule
{
    List<string> Check(BatchedPayBandTemplate item);
}

public sealed record BatchedPayBandTemplateDiscountRangeRule(double Min, double Max) : IBatchedPayBandTemplateRule
{
    public List<string> Check(BatchedPayBandTemplate item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedPayBandTemplateUpdatedAtRangeRule(int Min, int Max) : IBatchedPayBandTemplateRule
{
    public List<string> Check(BatchedPayBandTemplate item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayBandTemplateSalaryBandLengthRule(int MaxLength) : IBatchedPayBandTemplateRule
{
    public List<string> Check(BatchedPayBandTemplate item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched pay band template.</summary>
public sealed record BatchedPayBandTemplateValidator(List<IBatchedPayBandTemplateRule> Rules)
{
    public List<string> Validate(BatchedPayBandTemplate item)
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

    public void RequireValid(BatchedPayBandTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedPayBandTemplate> items)
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

/// <summary>Functions of batched pay band template rules.</summary>
public static class BatchedPayBandTemplateRules
{
    public static BatchedPayBandTemplateValidator DefaultBatchedPayBandTemplateValidator()
    {
        return new BatchedPayBandTemplateValidator(
            new List<IBatchedPayBandTemplateRule>
            {
                new BatchedPayBandTemplateDiscountRangeRule(1.4, 407.9),
                new BatchedPayBandTemplateUpdatedAtRangeRule(64, 1000),
                new BatchedPayBandTemplateSalaryBandLengthRule(1000),
            });
    }
}
