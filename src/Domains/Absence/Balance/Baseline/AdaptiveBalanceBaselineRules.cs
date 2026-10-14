namespace Acme.Workforce.Absence.Balance.Baseline;

/// <summary>A rule a adaptive balance baseline must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveBalanceBaselineRule
{
    List<string> Check(AdaptiveBalanceBaseline item);
}

public sealed record AdaptiveBalanceBaselineSourceLengthRule(int MaxLength) : IAdaptiveBalanceBaselineRule
{
    public List<string> Check(AdaptiveBalanceBaseline item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBalanceBaselineRequiredCodeRule(string Message) : IAdaptiveBalanceBaselineRule
{
    public List<string> Check(AdaptiveBalanceBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBalanceBaselineWindowDaysRangeRule(int Min, int Max) : IAdaptiveBalanceBaselineRule
{
    public List<string> Check(AdaptiveBalanceBaseline item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adaptive balance baseline.</summary>
public sealed record AdaptiveBalanceBaselineValidator(List<IAdaptiveBalanceBaselineRule> Rules)
{
    public List<string> Validate(AdaptiveBalanceBaseline item)
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

    public void RequireValid(AdaptiveBalanceBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveBalanceBaseline> items)
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

/// <summary>Functions of adaptive balance baseline rules.</summary>
public static class AdaptiveBalanceBaselineRules
{
    public static AdaptiveBalanceBaselineValidator DefaultAdaptiveBalanceBaselineValidator()
    {
        return new AdaptiveBalanceBaselineValidator(
            new List<IAdaptiveBalanceBaselineRule>
            {
                new AdaptiveBalanceBaselineSourceLengthRule(3600),
                new AdaptiveBalanceBaselineRequiredCodeRule("adaptive balance baseline needs this field"),
                new AdaptiveBalanceBaselineWindowDaysRangeRule(1000, 1000),
            });
    }
}
