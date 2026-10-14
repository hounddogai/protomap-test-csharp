namespace Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>A rule a default recognition policy must pass. Check returns its problems, or none.</summary>
public interface IDefaultRecognitionPolicyRule
{
    List<string> Check(DefaultRecognitionPolicy item);
}

public sealed record DefaultRecognitionPolicyRequiredNameRule(string Message) : IDefaultRecognitionPolicyRule
{
    public List<string> Check(DefaultRecognitionPolicy item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultRecognitionPolicyFloorValueRangeRule(double Min, double Max) : IDefaultRecognitionPolicyRule
{
    public List<string> Check(DefaultRecognitionPolicy item)
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

public sealed record DefaultRecognitionPolicyNameLengthRule(int MaxLength) : IDefaultRecognitionPolicyRule
{
    public List<string> Check(DefaultRecognitionPolicy item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRecognitionPolicyCodeLengthRule(int MaxLength) : IDefaultRecognitionPolicyRule
{
    public List<string> Check(DefaultRecognitionPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRecognitionPolicyRequiredSegmentRule(string Message) : IDefaultRecognitionPolicyRule
{
    public List<string> Check(DefaultRecognitionPolicy item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default recognition policy.</summary>
public sealed record DefaultRecognitionPolicyValidator(List<IDefaultRecognitionPolicyRule> Rules)
{
    public List<string> Validate(DefaultRecognitionPolicy item)
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

    public void RequireValid(DefaultRecognitionPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultRecognitionPolicy> items)
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

/// <summary>Functions of default recognition policy rules.</summary>
public static class DefaultRecognitionPolicyRules
{
    public static DefaultRecognitionPolicyValidator DefaultDefaultRecognitionPolicyValidator()
    {
        return new DefaultRecognitionPolicyValidator(
            new List<IDefaultRecognitionPolicyRule>
            {
                new DefaultRecognitionPolicyRequiredNameRule("default recognition policy needs this field"),
                new DefaultRecognitionPolicyFloorValueRangeRule(1.3, 775.5),
                new DefaultRecognitionPolicyNameLengthRule(3600),
                new DefaultRecognitionPolicyCodeLengthRule(1000),
                new DefaultRecognitionPolicyRequiredSegmentRule("default recognition policy needs this field"),
            });
    }
}
