namespace Acme.Workforce.Talent.Nomination.Template;

/// <summary>A rule a adaptive nomination target must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveNominationTargetRule
{
    List<string> Check(AdaptiveNominationTarget item);
}

public sealed record AdaptiveNominationTargetCodeLengthRule(int MaxLength) : IAdaptiveNominationTargetRule
{
    public List<string> Check(AdaptiveNominationTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveNominationTargetRequiredCodeRule(string Message) : IAdaptiveNominationTargetRule
{
    public List<string> Check(AdaptiveNominationTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveNominationTargetCostRangeRule(double Min, double Max) : IAdaptiveNominationTargetRule
{
    public List<string> Check(AdaptiveNominationTarget item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveNominationTargetGraceDaysRangeRule(int Min, int Max) : IAdaptiveNominationTargetRule
{
    public List<string> Check(AdaptiveNominationTarget item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adaptive nomination target.</summary>
public sealed record AdaptiveNominationTargetValidator(List<IAdaptiveNominationTargetRule> Rules)
{
    public List<string> Validate(AdaptiveNominationTarget item)
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

    public void RequireValid(AdaptiveNominationTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveNominationTarget> items)
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

/// <summary>Functions of adaptive nomination target rules.</summary>
public static class AdaptiveNominationTargetRules
{
    public static AdaptiveNominationTargetValidator DefaultAdaptiveNominationTargetValidator()
    {
        return new AdaptiveNominationTargetValidator(
            new List<IAdaptiveNominationTargetRule>
            {
                new AdaptiveNominationTargetCodeLengthRule(64),
                new AdaptiveNominationTargetRequiredCodeRule("adaptive nomination target needs an entry"),
                new AdaptiveNominationTargetCostRangeRule(3.1, 81.8),
                new AdaptiveNominationTargetGraceDaysRangeRule(255, 1000),
            });
    }
}
