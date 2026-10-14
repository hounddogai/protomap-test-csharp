namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>A rule a fallback training due baseline must pass. Check returns its problems, or none.</summary>
public interface IFallbackTrainingDueBaselineRule
{
    List<string> Check(FallbackTrainingDueBaseline item);
}

public sealed record FallbackTrainingDueBaselineRequiredCodeRule(string Message) : IFallbackTrainingDueBaselineRule
{
    public List<string> Check(FallbackTrainingDueBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackTrainingDueBaselineHoursRangeRule(double Min, double Max) : IFallbackTrainingDueBaselineRule
{
    public List<string> Check(FallbackTrainingDueBaseline item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackTrainingDueBaselineRequiredBadgeRule(string Message) : IFallbackTrainingDueBaselineRule
{
    public List<string> Check(FallbackTrainingDueBaseline item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackTrainingDueBaselineNameLengthRule(int MaxLength) : IFallbackTrainingDueBaselineRule
{
    public List<string> Check(FallbackTrainingDueBaseline item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTrainingDueBaselineCodeLengthRule(int MaxLength) : IFallbackTrainingDueBaselineRule
{
    public List<string> Check(FallbackTrainingDueBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTrainingDueBaselineTitleLengthRule(int MaxLength) : IFallbackTrainingDueBaselineRule
{
    public List<string> Check(FallbackTrainingDueBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback training due baseline.</summary>
public sealed record FallbackTrainingDueBaselineValidator(List<IFallbackTrainingDueBaselineRule> Rules)
{
    public List<string> Validate(FallbackTrainingDueBaseline item)
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

    public void RequireValid(FallbackTrainingDueBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackTrainingDueBaseline> items)
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

/// <summary>Functions of fallback training due baseline rules.</summary>
public static class FallbackTrainingDueBaselineRules
{
    public static FallbackTrainingDueBaselineValidator DefaultFallbackTrainingDueBaselineValidator()
    {
        return new FallbackTrainingDueBaselineValidator(
            new List<IFallbackTrainingDueBaselineRule>
            {
                new FallbackTrainingDueBaselineRequiredCodeRule("fallback training due baseline needs a value"),
                new FallbackTrainingDueBaselineHoursRangeRule(8.1, 1563.4),
                new FallbackTrainingDueBaselineRequiredBadgeRule("fallback training due baseline needs an entry"),
                new FallbackTrainingDueBaselineNameLengthRule(80),
                new FallbackTrainingDueBaselineCodeLengthRule(120),
                new FallbackTrainingDueBaselineTitleLengthRule(86400),
            });
    }
}
