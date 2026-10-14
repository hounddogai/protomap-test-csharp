namespace Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>A rule a position batch must pass. Check returns its problems, or none.</summary>
public interface IPositionBatchRule
{
    List<string> Check(PositionBatch item);
}

public sealed record PositionBatchCodeLengthRule(int MaxLength) : IPositionBatchRule
{
    public List<string> Check(PositionBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionBatchRequiredCodeRule(string Message) : IPositionBatchRule
{
    public List<string> Check(PositionBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionBatchScoreRangeRule(double Min, double Max) : IPositionBatchRule
{
    public List<string> Check(PositionBatch item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionBatchRetriesRangeRule(int Min, int Max) : IPositionBatchRule
{
    public List<string> Check(PositionBatch item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position batch.</summary>
public sealed record PositionBatchValidator(List<IPositionBatchRule> Rules)
{
    public List<string> Validate(PositionBatch item)
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

    public void RequireValid(PositionBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionBatch> items)
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

/// <summary>Functions of position batch rules.</summary>
public static class PositionBatchRules
{
    public static PositionBatchValidator DefaultPositionBatchValidator()
    {
        return new PositionBatchValidator(
            new List<IPositionBatchRule>
            {
                new PositionBatchCodeLengthRule(80),
                new PositionBatchRequiredCodeRule("position batch needs an entry"),
                new PositionBatchScoreRangeRule(4.5, 3923.8),
                new PositionBatchRetriesRangeRule(16, 3600),
            });
    }
}
