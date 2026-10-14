namespace Acme.Workforce.Engagement.Recognition.Allocation;

/// <summary>A rule a recognition budget must pass. Check returns its problems, or none.</summary>
public interface IRecognitionBudgetRule
{
    List<string> Check(RecognitionBudget item);
}

public sealed record RecognitionBudgetVersionRangeRule(int Min, int Max) : IRecognitionBudgetRule
{
    public List<string> Check(RecognitionBudget item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionBudgetCapacityRangeRule(int Min, int Max) : IRecognitionBudgetRule
{
    public List<string> Check(RecognitionBudget item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RecognitionBudgetRequiredCodeRule(string Message) : IRecognitionBudgetRule
{
    public List<string> Check(RecognitionBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionBudgetCodeLengthRule(int MaxLength) : IRecognitionBudgetRule
{
    public List<string> Check(RecognitionBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionBudgetRequiredLabelRule(string Message) : IRecognitionBudgetRule
{
    public List<string> Check(RecognitionBudget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition budget.</summary>
public sealed record RecognitionBudgetValidator(List<IRecognitionBudgetRule> Rules)
{
    public List<string> Validate(RecognitionBudget item)
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

    public void RequireValid(RecognitionBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionBudget> items)
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

/// <summary>Functions of recognition budget rules.</summary>
public static class RecognitionBudgetRules
{
    public static RecognitionBudgetValidator DefaultRecognitionBudgetValidator()
    {
        return new RecognitionBudgetValidator(
            new List<IRecognitionBudgetRule>
            {
                new RecognitionBudgetVersionRangeRule(1000, 86400),
                new RecognitionBudgetCapacityRangeRule(80, 120),
                new RecognitionBudgetRequiredCodeRule("recognition budget needs this field"),
                new RecognitionBudgetCodeLengthRule(80),
                new RecognitionBudgetRequiredLabelRule("recognition budget needs a value"),
            });
    }
}
