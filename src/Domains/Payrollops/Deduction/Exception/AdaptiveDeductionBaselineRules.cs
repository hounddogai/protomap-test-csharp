namespace Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>A rule a adaptive deduction baseline must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveDeductionBaselineRule
{
    List<string> Check(AdaptiveDeductionBaseline item);
}

public sealed record AdaptiveDeductionBaselineDurationMinutesRangeRule(int Min, int Max) : IAdaptiveDeductionBaselineRule
{
    public List<string> Check(AdaptiveDeductionBaseline item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeductionBaselineRequiredTitleRule(string Message) : IAdaptiveDeductionBaselineRule
{
    public List<string> Check(AdaptiveDeductionBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeductionBaselineCodeLengthRule(int MaxLength) : IAdaptiveDeductionBaselineRule
{
    public List<string> Check(AdaptiveDeductionBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeductionBaselineRequiredCodeRule(string Message) : IAdaptiveDeductionBaselineRule
{
    public List<string> Check(AdaptiveDeductionBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive deduction baseline.</summary>
public sealed record AdaptiveDeductionBaselineValidator(List<IAdaptiveDeductionBaselineRule> Rules)
{
    public List<string> Validate(AdaptiveDeductionBaseline item)
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

    public void RequireValid(AdaptiveDeductionBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveDeductionBaseline> items)
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

/// <summary>Functions of adaptive deduction baseline rules.</summary>
public static class AdaptiveDeductionBaselineRules
{
    public static AdaptiveDeductionBaselineValidator DefaultAdaptiveDeductionBaselineValidator()
    {
        return new AdaptiveDeductionBaselineValidator(
            new List<IAdaptiveDeductionBaselineRule>
            {
                new AdaptiveDeductionBaselineDurationMinutesRangeRule(16, 80),
                new AdaptiveDeductionBaselineRequiredTitleRule("adaptive deduction baseline needs a value"),
                new AdaptiveDeductionBaselineCodeLengthRule(3600),
                new AdaptiveDeductionBaselineRequiredCodeRule("adaptive deduction baseline needs this field"),
            });
    }
}
