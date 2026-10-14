namespace Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>A rule a recognition template must pass. Check returns its problems, or none.</summary>
public interface IRecognitionTemplateRule
{
    List<string> Check(RecognitionTemplate item);
}

public sealed record RecognitionTemplateRatioRangeRule(double Min, double Max) : IRecognitionTemplateRule
{
    public List<string> Check(RecognitionTemplate item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RecognitionTemplateRequiredManagerIdRule(string Message) : IRecognitionTemplateRule
{
    public List<string> Check(RecognitionTemplate item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionTemplateCodeLengthRule(int MaxLength) : IRecognitionTemplateRule
{
    public List<string> Check(RecognitionTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionTemplateNameLengthRule(int MaxLength) : IRecognitionTemplateRule
{
    public List<string> Check(RecognitionTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition template.</summary>
public sealed record RecognitionTemplateValidator(List<IRecognitionTemplateRule> Rules)
{
    public List<string> Validate(RecognitionTemplate item)
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

    public void RequireValid(RecognitionTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionTemplate> items)
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

/// <summary>Functions of recognition template rules.</summary>
public static class RecognitionTemplateRules
{
    public static RecognitionTemplateValidator DefaultRecognitionTemplateValidator()
    {
        return new RecognitionTemplateValidator(
            new List<IRecognitionTemplateRule>
            {
                new RecognitionTemplateRatioRangeRule(8.3, 1828.1),
                new RecognitionTemplateRequiredManagerIdRule("recognition template needs an entry"),
                new RecognitionTemplateCodeLengthRule(80),
                new RecognitionTemplateNameLengthRule(86400),
            });
    }
}
