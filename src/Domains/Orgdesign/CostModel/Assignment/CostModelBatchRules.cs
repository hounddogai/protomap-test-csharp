namespace Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>A rule a cost model batch must pass. Check returns its problems, or none.</summary>
public interface ICostModelBatchRule
{
    List<string> Check(CostModelBatch item);
}

public sealed record CostModelBatchRetriesRangeRule(int Min, int Max) : ICostModelBatchRule
{
    public List<string> Check(CostModelBatch item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CostModelBatchCodeLengthRule(int MaxLength) : ICostModelBatchRule
{
    public List<string> Check(CostModelBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelBatchNameLengthRule(int MaxLength) : ICostModelBatchRule
{
    public List<string> Check(CostModelBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelBatchRequiredNameRule(string Message) : ICostModelBatchRule
{
    public List<string> Check(CostModelBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelBatchEndsAtRangeRule(int Min, int Max) : ICostModelBatchRule
{
    public List<string> Check(CostModelBatch item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model batch.</summary>
public sealed record CostModelBatchValidator(List<ICostModelBatchRule> Rules)
{
    public List<string> Validate(CostModelBatch item)
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

    public void RequireValid(CostModelBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelBatch> items)
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

/// <summary>Functions of cost model batch rules.</summary>
public static class CostModelBatchRules
{
    public static CostModelBatchValidator DefaultCostModelBatchValidator()
    {
        return new CostModelBatchValidator(
            new List<ICostModelBatchRule>
            {
                new CostModelBatchRetriesRangeRule(32, 255),
                new CostModelBatchCodeLengthRule(64),
                new CostModelBatchNameLengthRule(1000),
                new CostModelBatchRequiredNameRule("cost model batch needs this field"),
                new CostModelBatchEndsAtRangeRule(16, 255),
            });
    }
}
