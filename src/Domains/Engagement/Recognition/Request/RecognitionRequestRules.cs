namespace Acme.Workforce.Engagement.Recognition.Request;

/// <summary>A rule a recognition request must pass. Check returns its problems, or none.</summary>
public interface IRecognitionRequestRule
{
    List<string> Check(RecognitionRequest item);
}

public sealed record RecognitionRequestCodeLengthRule(int MaxLength) : IRecognitionRequestRule
{
    public List<string> Check(RecognitionRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionRequestNotesLengthRule(int MaxLength) : IRecognitionRequestRule
{
    public List<string> Check(RecognitionRequest item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionRequestRequiredNotesRule(string Message) : IRecognitionRequestRule
{
    public List<string> Check(RecognitionRequest item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionRequestQuantityRangeRule(int Min, int Max) : IRecognitionRequestRule
{
    public List<string> Check(RecognitionRequest item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionRequestRateRangeRule(double Min, double Max) : IRecognitionRequestRule
{
    public List<string> Check(RecognitionRequest item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition request.</summary>
public sealed record RecognitionRequestValidator(List<IRecognitionRequestRule> Rules)
{
    public List<string> Validate(RecognitionRequest item)
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

    public void RequireValid(RecognitionRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionRequest> items)
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

/// <summary>Functions of recognition request rules.</summary>
public static class RecognitionRequestRules
{
    public static RecognitionRequestValidator DefaultRecognitionRequestValidator()
    {
        return new RecognitionRequestValidator(
            new List<IRecognitionRequestRule>
            {
                new RecognitionRequestCodeLengthRule(120),
                new RecognitionRequestNotesLengthRule(64),
                new RecognitionRequestRequiredNotesRule("recognition request needs an entry"),
                new RecognitionRequestQuantityRangeRule(32, 3600),
                new RecognitionRequestRateRangeRule(6.0, 2812.7),
            });
    }
}
