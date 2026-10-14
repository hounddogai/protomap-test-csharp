namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a recognition manifest must pass. Check returns its problems, or none.</summary>
public interface IRecognitionManifestRule
{
    List<string> Check(RecognitionManifest item);
}

public sealed record RecognitionManifestUnitLengthRule(int MaxLength) : IRecognitionManifestRule
{
    public List<string> Check(RecognitionManifest item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionManifestRatioRangeRule(double Min, double Max) : IRecognitionManifestRule
{
    public List<string> Check(RecognitionManifest item)
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

public sealed record RecognitionManifestAllowedTitleRule(List<string> Allowed) : IRecognitionManifestRule
{
    public List<string> Check(RecognitionManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is missing" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionManifestRequiredCodeRule(string Message) : IRecognitionManifestRule
{
    public List<string> Check(RecognitionManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition manifest.</summary>
public sealed record RecognitionManifestValidator(List<IRecognitionManifestRule> Rules)
{
    public List<string> Validate(RecognitionManifest item)
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

    public void RequireValid(RecognitionManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionManifest> items)
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

/// <summary>Functions of recognition manifest rules.</summary>
public static class RecognitionManifestRules
{
    public static RecognitionManifestValidator DefaultRecognitionManifestValidator()
    {
        return new RecognitionManifestValidator(
            new List<IRecognitionManifestRule>
            {
                new RecognitionManifestUnitLengthRule(3600),
                new RecognitionManifestRatioRangeRule(4.6, 2470.2),
                new RecognitionManifestAllowedTitleRule(new List<string> { "manual" }),
                new RecognitionManifestRequiredCodeRule("recognition manifest needs a value"),
            });
    }
}
