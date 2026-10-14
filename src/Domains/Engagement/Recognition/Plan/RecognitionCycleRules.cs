namespace Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>A rule a recognition cycle must pass. Check returns its problems, or none.</summary>
public interface IRecognitionCycleRule
{
    List<string> Check(RecognitionCycle item);
}

public sealed record RecognitionCycleMarginRangeRule(double Min, double Max) : IRecognitionCycleRule
{
    public List<string> Check(RecognitionCycle item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RecognitionCycleRequiredCodeRule(string Message) : IRecognitionCycleRule
{
    public List<string> Check(RecognitionCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionCycleGraceDaysRangeRule(int Min, int Max) : IRecognitionCycleRule
{
    public List<string> Check(RecognitionCycle item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition cycle.</summary>
public sealed record RecognitionCycleValidator(List<IRecognitionCycleRule> Rules)
{
    public List<string> Validate(RecognitionCycle item)
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

    public void RequireValid(RecognitionCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionCycle> items)
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

/// <summary>Functions of recognition cycle rules.</summary>
public static class RecognitionCycleRules
{
    public static RecognitionCycleValidator DefaultRecognitionCycleValidator()
    {
        return new RecognitionCycleValidator(
            new List<IRecognitionCycleRule>
            {
                new RecognitionCycleMarginRangeRule(8.1, 1756.0),
                new RecognitionCycleRequiredCodeRule("recognition cycle needs this field"),
                new RecognitionCycleGraceDaysRangeRule(32, 1000),
            });
    }
}
