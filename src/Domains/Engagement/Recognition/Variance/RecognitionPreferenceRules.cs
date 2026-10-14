namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a recognition preference must pass. Check returns its problems, or none.</summary>
public interface IRecognitionPreferenceRule
{
    List<string> Check(RecognitionPreference item);
}

public sealed record RecognitionPreferenceLabelLengthRule(int MaxLength) : IRecognitionPreferenceRule
{
    public List<string> Check(RecognitionPreference item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionPreferenceAllowedLabelRule(List<string> Allowed) : IRecognitionPreferenceRule
{
    public List<string> Check(RecognitionPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionPreferenceUnitLengthRule(int MaxLength) : IRecognitionPreferenceRule
{
    public List<string> Check(RecognitionPreference item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionPreferenceExpiresAtRangeRule(int Min, int Max) : IRecognitionPreferenceRule
{
    public List<string> Check(RecognitionPreference item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RecognitionPreferenceRequiredCodeRule(string Message) : IRecognitionPreferenceRule
{
    public List<string> Check(RecognitionPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionPreferenceEndsAtRangeRule(int Min, int Max) : IRecognitionPreferenceRule
{
    public List<string> Check(RecognitionPreference item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a recognition preference.</summary>
public sealed record RecognitionPreferenceValidator(List<IRecognitionPreferenceRule> Rules)
{
    public List<string> Validate(RecognitionPreference item)
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

    public void RequireValid(RecognitionPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionPreference> items)
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

/// <summary>Functions of recognition preference rules.</summary>
public static class RecognitionPreferenceRules
{
    public static RecognitionPreferenceValidator DefaultRecognitionPreferenceValidator()
    {
        return new RecognitionPreferenceValidator(
            new List<IRecognitionPreferenceRule>
            {
                new RecognitionPreferenceLabelLengthRule(120),
                new RecognitionPreferenceAllowedLabelRule(new List<string> { "pending", "batched", "global" }),
                new RecognitionPreferenceUnitLengthRule(86400),
                new RecognitionPreferenceExpiresAtRangeRule(16, 32),
                new RecognitionPreferenceRequiredCodeRule("recognition preference needs a value"),
                new RecognitionPreferenceEndsAtRangeRule(64, 255),
            });
    }
}
