namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>A rule a strict reorg estimate must pass. Check returns its problems, or none.</summary>
public interface IStrictReorgEstimateRule
{
    List<string> Check(StrictReorgEstimate item);
}

public sealed record StrictReorgEstimateRequiredCodeRule(string Message) : IStrictReorgEstimateRule
{
    public List<string> Check(StrictReorgEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReorgEstimateFloorValueRangeRule(double Min, double Max) : IStrictReorgEstimateRule
{
    public List<string> Check(StrictReorgEstimate item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictReorgEstimateCodeLengthRule(int MaxLength) : IStrictReorgEstimateRule
{
    public List<string> Check(StrictReorgEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictReorgEstimateAttemptsRangeRule(int Min, int Max) : IStrictReorgEstimateRule
{
    public List<string> Check(StrictReorgEstimate item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a strict reorg estimate.</summary>
public sealed record StrictReorgEstimateValidator(List<IStrictReorgEstimateRule> Rules)
{
    public List<string> Validate(StrictReorgEstimate item)
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

    public void RequireValid(StrictReorgEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictReorgEstimate> items)
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

/// <summary>Functions of strict reorg estimate rules.</summary>
public static class StrictReorgEstimateRules
{
    public static StrictReorgEstimateValidator DefaultStrictReorgEstimateValidator()
    {
        return new StrictReorgEstimateValidator(
            new List<IStrictReorgEstimateRule>
            {
                new StrictReorgEstimateRequiredCodeRule("strict reorg estimate needs this field"),
                new StrictReorgEstimateFloorValueRangeRule(8.5, 4919.2),
                new StrictReorgEstimateCodeLengthRule(86400),
                new StrictReorgEstimateAttemptsRangeRule(16, 86400),
            });
    }
}
