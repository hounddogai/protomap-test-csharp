namespace Acme.Workforce.Assets.Laptop.Review;

/// <summary>A rule a batched laptop variance must pass. Check returns its problems, or none.</summary>
public interface IBatchedLaptopVarianceRule
{
    List<string> Check(BatchedLaptopVariance item);
}

public sealed record BatchedLaptopVarianceRequiredGradeRule(string Message) : IBatchedLaptopVarianceRule
{
    public List<string> Check(BatchedLaptopVariance item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedLaptopVarianceFloorValueRangeRule(double Min, double Max) : IBatchedLaptopVarianceRule
{
    public List<string> Check(BatchedLaptopVariance item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLaptopVarianceRequiredTitleRule(string Message) : IBatchedLaptopVarianceRule
{
    public List<string> Check(BatchedLaptopVariance item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedLaptopVarianceRequiredCodeRule(string Message) : IBatchedLaptopVarianceRule
{
    public List<string> Check(BatchedLaptopVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedLaptopVarianceTitleLengthRule(int MaxLength) : IBatchedLaptopVarianceRule
{
    public List<string> Check(BatchedLaptopVariance item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLaptopVarianceRateRangeRule(double Min, double Max) : IBatchedLaptopVarianceRule
{
    public List<string> Check(BatchedLaptopVariance item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a batched laptop variance.</summary>
public sealed record BatchedLaptopVarianceValidator(List<IBatchedLaptopVarianceRule> Rules)
{
    public List<string> Validate(BatchedLaptopVariance item)
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

    public void RequireValid(BatchedLaptopVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedLaptopVariance> items)
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

/// <summary>Functions of batched laptop variance rules.</summary>
public static class BatchedLaptopVarianceRules
{
    public static BatchedLaptopVarianceValidator DefaultBatchedLaptopVarianceValidator()
    {
        return new BatchedLaptopVarianceValidator(
            new List<IBatchedLaptopVarianceRule>
            {
                new BatchedLaptopVarianceRequiredGradeRule("batched laptop variance needs this field"),
                new BatchedLaptopVarianceFloorValueRangeRule(8.4, 3856.6),
                new BatchedLaptopVarianceRequiredTitleRule("batched laptop variance needs this field"),
                new BatchedLaptopVarianceRequiredCodeRule("batched laptop variance needs an entry"),
                new BatchedLaptopVarianceTitleLengthRule(32),
                new BatchedLaptopVarianceRateRangeRule(7.3, 3110.7),
            });
    }
}
