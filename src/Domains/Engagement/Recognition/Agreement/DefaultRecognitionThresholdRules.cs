namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default recognition threshold must pass. Check returns its problems, or none.</summary>
public interface IDefaultRecognitionThresholdRule
{
    List<string> Check(DefaultRecognitionThreshold item);
}

public sealed record DefaultRecognitionThresholdSalaryBandLengthRule(int MaxLength) : IDefaultRecognitionThresholdRule
{
    public List<string> Check(DefaultRecognitionThreshold item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRecognitionThresholdRequiredChannelRule(string Message) : IDefaultRecognitionThresholdRule
{
    public List<string> Check(DefaultRecognitionThreshold item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultRecognitionThresholdAllowedChannelRule(List<string> Allowed) : IDefaultRecognitionThresholdRule
{
    public List<string> Check(DefaultRecognitionThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is stale" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRecognitionThresholdRequiredSalaryBandRule(string Message) : IDefaultRecognitionThresholdRule
{
    public List<string> Check(DefaultRecognitionThreshold item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultRecognitionThresholdChannelLengthRule(int MaxLength) : IDefaultRecognitionThresholdRule
{
    public List<string> Check(DefaultRecognitionThreshold item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRecognitionThresholdRatingRangeRule(double Min, double Max) : IDefaultRecognitionThresholdRule
{
    public List<string> Check(DefaultRecognitionThreshold item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default recognition threshold.</summary>
public sealed record DefaultRecognitionThresholdValidator(List<IDefaultRecognitionThresholdRule> Rules)
{
    public List<string> Validate(DefaultRecognitionThreshold item)
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

    public void RequireValid(DefaultRecognitionThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultRecognitionThreshold> items)
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

/// <summary>Functions of default recognition threshold rules.</summary>
public static class DefaultRecognitionThresholdRules
{
    public static DefaultRecognitionThresholdValidator DefaultDefaultRecognitionThresholdValidator()
    {
        return new DefaultRecognitionThresholdValidator(
            new List<IDefaultRecognitionThresholdRule>
            {
                new DefaultRecognitionThresholdSalaryBandLengthRule(32),
                new DefaultRecognitionThresholdRequiredChannelRule("default recognition threshold needs this field"),
                new DefaultRecognitionThresholdAllowedChannelRule(new List<string> { "global", "lenient" }),
                new DefaultRecognitionThresholdRequiredSalaryBandRule("default recognition threshold needs a value"),
                new DefaultRecognitionThresholdChannelLengthRule(86400),
                new DefaultRecognitionThresholdRatingRangeRule(2.9, 4469.3),
            });
    }
}
