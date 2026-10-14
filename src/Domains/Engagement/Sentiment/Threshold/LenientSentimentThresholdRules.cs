namespace Acme.Workforce.Engagement.Sentiment.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient sentiment threshold must pass. Check returns its problems, or none.</summary>
public interface ILenientSentimentThresholdRule
{
    List<string> Check(LenientSentimentThreshold item);
}

public sealed record LenientSentimentThresholdFloorValueRangeRule(double Min, double Max) : ILenientSentimentThresholdRule
{
    public List<string> Check(LenientSentimentThreshold item)
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

public sealed record LenientSentimentThresholdDurationMinutesRangeRule(int Min, int Max) : ILenientSentimentThresholdRule
{
    public List<string> Check(LenientSentimentThreshold item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentThresholdSalaryBandLengthRule(int MaxLength) : ILenientSentimentThresholdRule
{
    public List<string> Check(LenientSentimentThreshold item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentThresholdRequiredCountryRule(string Message) : ILenientSentimentThresholdRule
{
    public List<string> Check(LenientSentimentThreshold item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentThresholdCodeLengthRule(int MaxLength) : ILenientSentimentThresholdRule
{
    public List<string> Check(LenientSentimentThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentThresholdAllowedLabelRule(List<string> Allowed) : ILenientSentimentThresholdRule
{
    public List<string> Check(LenientSentimentThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient sentiment threshold.</summary>
public sealed record LenientSentimentThresholdValidator(List<ILenientSentimentThresholdRule> Rules)
{
    public List<string> Validate(LenientSentimentThreshold item)
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

    public void RequireValid(LenientSentimentThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientSentimentThreshold> items)
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

/// <summary>Functions of lenient sentiment threshold rules.</summary>
public static class LenientSentimentThresholdRules
{
    public static LenientSentimentThresholdValidator DefaultLenientSentimentThresholdValidator()
    {
        return new LenientSentimentThresholdValidator(
            new List<ILenientSentimentThresholdRule>
            {
                new LenientSentimentThresholdFloorValueRangeRule(5.0, 977.0),
                new LenientSentimentThresholdDurationMinutesRangeRule(16, 255),
                new LenientSentimentThresholdSalaryBandLengthRule(32),
                new LenientSentimentThresholdRequiredCountryRule("lenient sentiment threshold needs an entry"),
                new LenientSentimentThresholdCodeLengthRule(32),
                new LenientSentimentThresholdAllowedLabelRule(new List<string> { "batched", "capped" }),
            });
    }
}
