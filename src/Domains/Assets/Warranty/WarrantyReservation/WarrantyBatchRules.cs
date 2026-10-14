namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>A rule a warranty batch must pass. Check returns its problems, or none.</summary>
public interface IWarrantyBatchRule
{
    List<string> Check(WarrantyBatch item);
}

public sealed record WarrantyBatchRequiredNameRule(string Message) : IWarrantyBatchRule
{
    public List<string> Check(WarrantyBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyBatchPriorityRangeRule(int Min, int Max) : IWarrantyBatchRule
{
    public List<string> Check(WarrantyBatch item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WarrantyBatchRequiredCodeRule(string Message) : IWarrantyBatchRule
{
    public List<string> Check(WarrantyBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyBatchDescriptionLengthRule(int MaxLength) : IWarrantyBatchRule
{
    public List<string> Check(WarrantyBatch item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyBatchRetriesRangeRule(int Min, int Max) : IWarrantyBatchRule
{
    public List<string> Check(WarrantyBatch item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyBatchNameLengthRule(int MaxLength) : IWarrantyBatchRule
{
    public List<string> Check(WarrantyBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty batch.</summary>
public sealed record WarrantyBatchValidator(List<IWarrantyBatchRule> Rules)
{
    public List<string> Validate(WarrantyBatch item)
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

    public void RequireValid(WarrantyBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyBatch> items)
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

/// <summary>Functions of warranty batch rules.</summary>
public static class WarrantyBatchRules
{
    public static WarrantyBatchValidator DefaultWarrantyBatchValidator()
    {
        return new WarrantyBatchValidator(
            new List<IWarrantyBatchRule>
            {
                new WarrantyBatchRequiredNameRule("warranty batch needs this field"),
                new WarrantyBatchPriorityRangeRule(16, 80),
                new WarrantyBatchRequiredCodeRule("warranty batch needs this field"),
                new WarrantyBatchDescriptionLengthRule(86400),
                new WarrantyBatchRetriesRangeRule(255, 3600),
                new WarrantyBatchNameLengthRule(86400),
            });
    }
}
