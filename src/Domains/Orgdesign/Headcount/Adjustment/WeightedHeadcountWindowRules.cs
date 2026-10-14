namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>A rule a weighted headcount window must pass. Check returns its problems, or none.</summary>
public interface IWeightedHeadcountWindowRule
{
    List<string> Check(WeightedHeadcountWindow item);
}

public sealed record WeightedHeadcountWindowWeightRangeRule(double Min, double Max) : IWeightedHeadcountWindowRule
{
    public List<string> Check(WeightedHeadcountWindow item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedHeadcountWindowRequiredManagerIdRule(string Message) : IWeightedHeadcountWindowRule
{
    public List<string> Check(WeightedHeadcountWindow item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedHeadcountWindowManagerIdLengthRule(int MaxLength) : IWeightedHeadcountWindowRule
{
    public List<string> Check(WeightedHeadcountWindow item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedHeadcountWindowAttemptsRangeRule(int Min, int Max) : IWeightedHeadcountWindowRule
{
    public List<string> Check(WeightedHeadcountWindow item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted headcount window.</summary>
public sealed record WeightedHeadcountWindowValidator(List<IWeightedHeadcountWindowRule> Rules)
{
    public List<string> Validate(WeightedHeadcountWindow item)
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

    public void RequireValid(WeightedHeadcountWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedHeadcountWindow> items)
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

/// <summary>Functions of weighted headcount window rules.</summary>
public static class WeightedHeadcountWindowRules
{
    public static WeightedHeadcountWindowValidator DefaultWeightedHeadcountWindowValidator()
    {
        return new WeightedHeadcountWindowValidator(
            new List<IWeightedHeadcountWindowRule>
            {
                new WeightedHeadcountWindowWeightRangeRule(8.3, 3531.8),
                new WeightedHeadcountWindowRequiredManagerIdRule("weighted headcount window needs this field"),
                new WeightedHeadcountWindowManagerIdLengthRule(255),
                new WeightedHeadcountWindowAttemptsRangeRule(255, 3600),
            });
    }
}
