namespace Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>A rule a adaptive nomination variance must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveNominationVarianceRule
{
    List<string> Check(AdaptiveNominationVariance item);
}

public sealed record AdaptiveNominationVarianceRequiredSourceRule(string Message) : IAdaptiveNominationVarianceRule
{
    public List<string> Check(AdaptiveNominationVariance item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveNominationVarianceRequiredLabelRule(string Message) : IAdaptiveNominationVarianceRule
{
    public List<string> Check(AdaptiveNominationVariance item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveNominationVariancePriorityRangeRule(int Min, int Max) : IAdaptiveNominationVarianceRule
{
    public List<string> Check(AdaptiveNominationVariance item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveNominationVarianceCodeLengthRule(int MaxLength) : IAdaptiveNominationVarianceRule
{
    public List<string> Check(AdaptiveNominationVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive nomination variance.</summary>
public sealed record AdaptiveNominationVarianceValidator(List<IAdaptiveNominationVarianceRule> Rules)
{
    public List<string> Validate(AdaptiveNominationVariance item)
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

    public void RequireValid(AdaptiveNominationVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveNominationVariance> items)
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

/// <summary>Functions of adaptive nomination variance rules.</summary>
public static class AdaptiveNominationVarianceRules
{
    public static AdaptiveNominationVarianceValidator DefaultAdaptiveNominationVarianceValidator()
    {
        return new AdaptiveNominationVarianceValidator(
            new List<IAdaptiveNominationVarianceRule>
            {
                new AdaptiveNominationVarianceRequiredSourceRule("adaptive nomination variance needs an entry"),
                new AdaptiveNominationVarianceRequiredLabelRule("adaptive nomination variance needs a value"),
                new AdaptiveNominationVariancePriorityRangeRule(64, 80),
                new AdaptiveNominationVarianceCodeLengthRule(64),
            });
    }
}
