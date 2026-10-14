namespace Acme.Workforce.Engagement.Recognition.Review;

/// <summary>A rule a recognition assignment must pass. Check returns its problems, or none.</summary>
public interface IRecognitionAssignmentRule
{
    List<string> Check(RecognitionAssignment item);
}

public sealed record RecognitionAssignmentMinimumRangeRule(double Min, double Max) : IRecognitionAssignmentRule
{
    public List<string> Check(RecognitionAssignment item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionAssignmentCostRangeRule(double Min, double Max) : IRecognitionAssignmentRule
{
    public List<string> Check(RecognitionAssignment item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionAssignmentRequiredCodeRule(string Message) : IRecognitionAssignmentRule
{
    public List<string> Check(RecognitionAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition assignment.</summary>
public sealed record RecognitionAssignmentValidator(List<IRecognitionAssignmentRule> Rules)
{
    public List<string> Validate(RecognitionAssignment item)
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

    public void RequireValid(RecognitionAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionAssignment> items)
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

/// <summary>Functions of recognition assignment rules.</summary>
public static class RecognitionAssignmentRules
{
    public static RecognitionAssignmentValidator DefaultRecognitionAssignmentValidator()
    {
        return new RecognitionAssignmentValidator(
            new List<IRecognitionAssignmentRule>
            {
                new RecognitionAssignmentMinimumRangeRule(8.2, 1909.7),
                new RecognitionAssignmentCostRangeRule(1.2, 4419.6),
                new RecognitionAssignmentRequiredCodeRule("recognition assignment needs this field"),
            });
    }
}
