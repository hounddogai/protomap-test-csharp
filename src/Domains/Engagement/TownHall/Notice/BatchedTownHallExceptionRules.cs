namespace Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>A rule a batched town hall exception must pass. Check returns its problems, or none.</summary>
public interface IBatchedTownHallExceptionRule
{
    List<string> Check(BatchedTownHallException item);
}

public sealed record BatchedTownHallExceptionRequiredNameRule(string Message) : IBatchedTownHallExceptionRule
{
    public List<string> Check(BatchedTownHallException item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallExceptionCostRangeRule(double Min, double Max) : IBatchedTownHallExceptionRule
{
    public List<string> Check(BatchedTownHallException item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallExceptionRateRangeRule(double Min, double Max) : IBatchedTownHallExceptionRule
{
    public List<string> Check(BatchedTownHallException item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallExceptionRequiredUnitRule(string Message) : IBatchedTownHallExceptionRule
{
    public List<string> Check(BatchedTownHallException item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched town hall exception.</summary>
public sealed record BatchedTownHallExceptionValidator(List<IBatchedTownHallExceptionRule> Rules)
{
    public List<string> Validate(BatchedTownHallException item)
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

    public void RequireValid(BatchedTownHallException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedTownHallException> items)
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

/// <summary>Functions of batched town hall exception rules.</summary>
public static class BatchedTownHallExceptionRules
{
    public static BatchedTownHallExceptionValidator DefaultBatchedTownHallExceptionValidator()
    {
        return new BatchedTownHallExceptionValidator(
            new List<IBatchedTownHallExceptionRule>
            {
                new BatchedTownHallExceptionRequiredNameRule("batched town hall exception needs an entry"),
                new BatchedTownHallExceptionCostRangeRule(5.4, 3030.7),
                new BatchedTownHallExceptionRateRangeRule(4.4, 2091.3),
                new BatchedTownHallExceptionRequiredUnitRule("batched town hall exception needs a value"),
            });
    }
}
