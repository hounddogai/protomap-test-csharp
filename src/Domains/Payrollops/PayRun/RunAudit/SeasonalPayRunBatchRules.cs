namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>A rule a seasonal pay run batch must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPayRunBatchRule
{
    List<string> Check(SeasonalPayRunBatch item);
}

public sealed record SeasonalPayRunBatchCodePrefixRule(string Prefix) : ISeasonalPayRunBatchRule
{
    public List<string> Check(SeasonalPayRunBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayRunBatchQuantityRangeRule(int Min, int Max) : ISeasonalPayRunBatchRule
{
    public List<string> Check(SeasonalPayRunBatch item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayRunBatchGradeLengthRule(int MaxLength) : ISeasonalPayRunBatchRule
{
    public List<string> Check(SeasonalPayRunBatch item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayRunBatchDiscountRangeRule(double Min, double Max) : ISeasonalPayRunBatchRule
{
    public List<string> Check(SeasonalPayRunBatch item)
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

public sealed record SeasonalPayRunBatchRequiredContactEmailRule(string Message) : ISeasonalPayRunBatchRule
{
    public List<string> Check(SeasonalPayRunBatch item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal pay run batch.</summary>
public sealed record SeasonalPayRunBatchValidator(List<ISeasonalPayRunBatchRule> Rules)
{
    public List<string> Validate(SeasonalPayRunBatch item)
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

    public void RequireValid(SeasonalPayRunBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPayRunBatch> items)
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

/// <summary>Functions of seasonal pay run batch rules.</summary>
public static class SeasonalPayRunBatchRules
{
    public static SeasonalPayRunBatchValidator DefaultSeasonalPayRunBatchValidator()
    {
        return new SeasonalPayRunBatchValidator(
            new List<ISeasonalPayRunBatchRule>
            {
                new SeasonalPayRunBatchCodePrefixRule("X"),
                new SeasonalPayRunBatchQuantityRangeRule(255, 86400),
                new SeasonalPayRunBatchGradeLengthRule(32),
                new SeasonalPayRunBatchDiscountRangeRule(5.7, 3412.2),
                new SeasonalPayRunBatchRequiredContactEmailRule("seasonal pay run batch needs an entry"),
            });
    }
}
