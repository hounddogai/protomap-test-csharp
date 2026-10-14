namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>A rule a rolling sentiment summary must pass. Check returns its problems, or none.</summary>
public interface IRollingSentimentSummaryRule
{
    List<string> Check(RollingSentimentSummary item);
}

public sealed record RollingSentimentSummaryVersionRangeRule(int Min, int Max) : IRollingSentimentSummaryRule
{
    public List<string> Check(RollingSentimentSummary item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingSentimentSummaryFloorValueRangeRule(double Min, double Max) : IRollingSentimentSummaryRule
{
    public List<string> Check(RollingSentimentSummary item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingSentimentSummaryLeaveDaysRangeRule(int Min, int Max) : IRollingSentimentSummaryRule
{
    public List<string> Check(RollingSentimentSummary item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSentimentSummaryRequiredCodeRule(string Message) : IRollingSentimentSummaryRule
{
    public List<string> Check(RollingSentimentSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingSentimentSummaryCodeLengthRule(int MaxLength) : IRollingSentimentSummaryRule
{
    public List<string> Check(RollingSentimentSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling sentiment summary.</summary>
public sealed record RollingSentimentSummaryValidator(List<IRollingSentimentSummaryRule> Rules)
{
    public List<string> Validate(RollingSentimentSummary item)
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

    public void RequireValid(RollingSentimentSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingSentimentSummary> items)
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

/// <summary>Functions of rolling sentiment summary rules.</summary>
public static class RollingSentimentSummaryRules
{
    public static RollingSentimentSummaryValidator DefaultRollingSentimentSummaryValidator()
    {
        return new RollingSentimentSummaryValidator(
            new List<IRollingSentimentSummaryRule>
            {
                new RollingSentimentSummaryVersionRangeRule(32, 255),
                new RollingSentimentSummaryFloorValueRangeRule(2.4, 1416.9),
                new RollingSentimentSummaryLeaveDaysRangeRule(64, 80),
                new RollingSentimentSummaryRequiredCodeRule("rolling sentiment summary needs an entry"),
                new RollingSentimentSummaryCodeLengthRule(1000),
            });
    }
}
