namespace Acme.Workforce.Engagement.Recognition.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a recognition window must pass. Check returns its problems, or none.</summary>
public interface IRecognitionWindowRule
{
    List<string> Check(RecognitionWindow item);
}

public sealed record RecognitionWindowQuantityRangeRule(int Min, int Max) : IRecognitionWindowRule
{
    public List<string> Check(RecognitionWindow item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RecognitionWindowAllowedManagerIdRule(List<string> Allowed) : IRecognitionWindowRule
{
    public List<string> Check(RecognitionWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionWindowRequiredCodeRule(string Message) : IRecognitionWindowRule
{
    public List<string> Check(RecognitionWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition window.</summary>
public sealed record RecognitionWindowValidator(List<IRecognitionWindowRule> Rules)
{
    public List<string> Validate(RecognitionWindow item)
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

    public void RequireValid(RecognitionWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionWindow> items)
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

/// <summary>Functions of recognition window rules.</summary>
public static class RecognitionWindowRules
{
    public static RecognitionWindowValidator DefaultRecognitionWindowValidator()
    {
        return new RecognitionWindowValidator(
            new List<IRecognitionWindowRule>
            {
                new RecognitionWindowQuantityRangeRule(255, 86400),
                new RecognitionWindowAllowedManagerIdRule(new List<string> { "legacy", "lenient" }),
                new RecognitionWindowRequiredCodeRule("recognition window needs an entry"),
            });
    }
}
