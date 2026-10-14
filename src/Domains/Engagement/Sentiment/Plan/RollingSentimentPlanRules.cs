namespace Acme.Workforce.Engagement.Sentiment.Plan;

/// <summary>A rule a rolling sentiment plan must pass. Check returns its problems, or none.</summary>
public interface IRollingSentimentPlanRule
{
    List<string> Check(RollingSentimentPlan item);
}

public sealed record RollingSentimentPlanCodeLengthRule(int MaxLength) : IRollingSentimentPlanRule
{
    public List<string> Check(RollingSentimentPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSentimentPlanNameLengthRule(int MaxLength) : IRollingSentimentPlanRule
{
    public List<string> Check(RollingSentimentPlan item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSentimentPlanMinimumRangeRule(double Min, double Max) : IRollingSentimentPlanRule
{
    public List<string> Check(RollingSentimentPlan item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSentimentPlanRetriesRangeRule(int Min, int Max) : IRollingSentimentPlanRule
{
    public List<string> Check(RollingSentimentPlan item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSentimentPlanThresholdRangeRule(double Min, double Max) : IRollingSentimentPlanRule
{
    public List<string> Check(RollingSentimentPlan item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling sentiment plan.</summary>
public sealed record RollingSentimentPlanValidator(List<IRollingSentimentPlanRule> Rules)
{
    public List<string> Validate(RollingSentimentPlan item)
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

    public void RequireValid(RollingSentimentPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingSentimentPlan> items)
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

/// <summary>Functions of rolling sentiment plan rules.</summary>
public static class RollingSentimentPlanRules
{
    public static RollingSentimentPlanValidator DefaultRollingSentimentPlanValidator()
    {
        return new RollingSentimentPlanValidator(
            new List<IRollingSentimentPlanRule>
            {
                new RollingSentimentPlanCodeLengthRule(80),
                new RollingSentimentPlanNameLengthRule(120),
                new RollingSentimentPlanMinimumRangeRule(4.7, 3689.9),
                new RollingSentimentPlanRetriesRangeRule(64, 64),
                new RollingSentimentPlanThresholdRangeRule(1.0, 2198.1),
            });
    }
}
