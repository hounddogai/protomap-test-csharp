namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a recognition threshold must pass. Check returns its problems, or none.</summary>
public interface IRecognitionThresholdRule
{
    List<string> Check(RecognitionThreshold item);
}

public sealed record RecognitionThresholdSourceLengthRule(int MaxLength) : IRecognitionThresholdRule
{
    public List<string> Check(RecognitionThreshold item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionThresholdAllowedLabelRule(List<string> Allowed) : IRecognitionThresholdRule
{
    public List<string> Check(RecognitionThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionThresholdRequiredTimezoneRule(string Message) : IRecognitionThresholdRule
{
    public List<string> Check(RecognitionThreshold item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition threshold.</summary>
public sealed record RecognitionThresholdValidator(List<IRecognitionThresholdRule> Rules)
{
    public List<string> Validate(RecognitionThreshold item)
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

    public void RequireValid(RecognitionThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionThreshold> items)
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

/// <summary>Functions of recognition threshold rules.</summary>
public static class RecognitionThresholdRules
{
    public static RecognitionThresholdValidator DefaultRecognitionThresholdValidator()
    {
        return new RecognitionThresholdValidator(
            new List<IRecognitionThresholdRule>
            {
                new RecognitionThresholdSourceLengthRule(120),
                new RecognitionThresholdAllowedLabelRule(new List<string> { "fallback", "regional" }),
                new RecognitionThresholdRequiredTimezoneRule("recognition threshold needs an entry"),
            });
    }
}
