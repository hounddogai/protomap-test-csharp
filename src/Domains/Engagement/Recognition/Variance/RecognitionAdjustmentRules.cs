namespace Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>A rule a recognition adjustment must pass. Check returns its problems, or none.</summary>
public interface IRecognitionAdjustmentRule
{
    List<string> Check(RecognitionAdjustment item);
}

public sealed record RecognitionAdjustmentRequiredNameRule(string Message) : IRecognitionAdjustmentRule
{
    public List<string> Check(RecognitionAdjustment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionAdjustmentCostRangeRule(double Min, double Max) : IRecognitionAdjustmentRule
{
    public List<string> Check(RecognitionAdjustment item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionAdjustmentDescriptionLengthRule(int MaxLength) : IRecognitionAdjustmentRule
{
    public List<string> Check(RecognitionAdjustment item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionAdjustmentRequiredDescriptionRule(string Message) : IRecognitionAdjustmentRule
{
    public List<string> Check(RecognitionAdjustment item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition adjustment.</summary>
public sealed record RecognitionAdjustmentValidator(List<IRecognitionAdjustmentRule> Rules)
{
    public List<string> Validate(RecognitionAdjustment item)
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

    public void RequireValid(RecognitionAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionAdjustment> items)
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

/// <summary>Functions of recognition adjustment rules.</summary>
public static class RecognitionAdjustmentRules
{
    public static RecognitionAdjustmentValidator DefaultRecognitionAdjustmentValidator()
    {
        return new RecognitionAdjustmentValidator(
            new List<IRecognitionAdjustmentRule>
            {
                new RecognitionAdjustmentRequiredNameRule("recognition adjustment needs an entry"),
                new RecognitionAdjustmentCostRangeRule(5.2, 4035.0),
                new RecognitionAdjustmentDescriptionLengthRule(3600),
                new RecognitionAdjustmentRequiredDescriptionRule("recognition adjustment needs a value"),
            });
    }
}
