namespace Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>A rule a succession plan window must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanWindowRule
{
    List<string> Check(SuccessionPlanWindow item);
}

public sealed record SuccessionPlanWindowLocaleLengthRule(int MaxLength) : ISuccessionPlanWindowRule
{
    public List<string> Check(SuccessionPlanWindow item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanWindowQuantityRangeRule(int Min, int Max) : ISuccessionPlanWindowRule
{
    public List<string> Check(SuccessionPlanWindow item)
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

public sealed record SuccessionPlanWindowSummaryLengthRule(int MaxLength) : ISuccessionPlanWindowRule
{
    public List<string> Check(SuccessionPlanWindow item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan window.</summary>
public sealed record SuccessionPlanWindowValidator(List<ISuccessionPlanWindowRule> Rules)
{
    public List<string> Validate(SuccessionPlanWindow item)
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

    public void RequireValid(SuccessionPlanWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanWindow> items)
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

/// <summary>Functions of succession plan window rules.</summary>
public static class SuccessionPlanWindowRules
{
    public static SuccessionPlanWindowValidator DefaultSuccessionPlanWindowValidator()
    {
        return new SuccessionPlanWindowValidator(
            new List<ISuccessionPlanWindowRule>
            {
                new SuccessionPlanWindowLocaleLengthRule(32),
                new SuccessionPlanWindowQuantityRangeRule(16, 255),
                new SuccessionPlanWindowSummaryLengthRule(80),
            });
    }
}
