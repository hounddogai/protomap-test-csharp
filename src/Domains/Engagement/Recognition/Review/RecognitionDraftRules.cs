namespace Acme.Workforce.Engagement.Recognition.Review;

/// <summary>A rule a recognition draft must pass. Check returns its problems, or none.</summary>
public interface IRecognitionDraftRule
{
    List<string> Check(RecognitionDraft item);
}

public sealed record RecognitionDraftRequiredCodeRule(string Message) : IRecognitionDraftRule
{
    public List<string> Check(RecognitionDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionDraftUpdatedAtRangeRule(int Min, int Max) : IRecognitionDraftRule
{
    public List<string> Check(RecognitionDraft item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionDraftSegmentLengthRule(int MaxLength) : IRecognitionDraftRule
{
    public List<string> Check(RecognitionDraft item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionDraftNameLengthRule(int MaxLength) : IRecognitionDraftRule
{
    public List<string> Check(RecognitionDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionDraftRequiredNameRule(string Message) : IRecognitionDraftRule
{
    public List<string> Check(RecognitionDraft item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition draft.</summary>
public sealed record RecognitionDraftValidator(List<IRecognitionDraftRule> Rules)
{
    public List<string> Validate(RecognitionDraft item)
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

    public void RequireValid(RecognitionDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionDraft> items)
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

/// <summary>Functions of recognition draft rules.</summary>
public static class RecognitionDraftRules
{
    public static RecognitionDraftValidator DefaultRecognitionDraftValidator()
    {
        return new RecognitionDraftValidator(
            new List<IRecognitionDraftRule>
            {
                new RecognitionDraftRequiredCodeRule("recognition draft needs a value"),
                new RecognitionDraftUpdatedAtRangeRule(80, 1000),
                new RecognitionDraftSegmentLengthRule(3600),
                new RecognitionDraftNameLengthRule(255),
                new RecognitionDraftRequiredNameRule("recognition draft needs a value"),
            });
    }
}
