namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

/// <summary>A rule a adaptive reorg baseline must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveReorgBaselineRule
{
    List<string> Check(AdaptiveReorgBaseline item);
}

public sealed record AdaptiveReorgBaselineNameLengthRule(int MaxLength) : IAdaptiveReorgBaselineRule
{
    public List<string> Check(AdaptiveReorgBaseline item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveReorgBaselineRequiredCodeRule(string Message) : IAdaptiveReorgBaselineRule
{
    public List<string> Check(AdaptiveReorgBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveReorgBaselineRequiredTimezoneRule(string Message) : IAdaptiveReorgBaselineRule
{
    public List<string> Check(AdaptiveReorgBaseline item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveReorgBaselineRetriesRangeRule(int Min, int Max) : IAdaptiveReorgBaselineRule
{
    public List<string> Check(AdaptiveReorgBaseline item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adaptive reorg baseline.</summary>
public sealed record AdaptiveReorgBaselineValidator(List<IAdaptiveReorgBaselineRule> Rules)
{
    public List<string> Validate(AdaptiveReorgBaseline item)
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

    public void RequireValid(AdaptiveReorgBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveReorgBaseline> items)
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

/// <summary>Functions of adaptive reorg baseline rules.</summary>
public static class AdaptiveReorgBaselineRules
{
    public static AdaptiveReorgBaselineValidator DefaultAdaptiveReorgBaselineValidator()
    {
        return new AdaptiveReorgBaselineValidator(
            new List<IAdaptiveReorgBaselineRule>
            {
                new AdaptiveReorgBaselineNameLengthRule(255),
                new AdaptiveReorgBaselineRequiredCodeRule("adaptive reorg baseline needs an entry"),
                new AdaptiveReorgBaselineRequiredTimezoneRule("adaptive reorg baseline needs a value"),
                new AdaptiveReorgBaselineRetriesRangeRule(16, 1000),
            });
    }
}
