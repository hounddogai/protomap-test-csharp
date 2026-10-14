namespace Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>A rule a seasonal bonus pool request must pass. Check returns its problems, or none.</summary>
public interface ISeasonalBonusPoolRequestRule
{
    List<string> Check(SeasonalBonusPoolRequest item);
}

public sealed record SeasonalBonusPoolRequestCeilingRangeRule(double Min, double Max) : ISeasonalBonusPoolRequestRule
{
    public List<string> Check(SeasonalBonusPoolRequest item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalBonusPoolRequestCodePrefixRule(string Prefix) : ISeasonalBonusPoolRequestRule
{
    public List<string> Check(SeasonalBonusPoolRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBonusPoolRequestScoreRangeRule(double Min, double Max) : ISeasonalBonusPoolRequestRule
{
    public List<string> Check(SeasonalBonusPoolRequest item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalBonusPoolRequestRateRangeRule(double Min, double Max) : ISeasonalBonusPoolRequestRule
{
    public List<string> Check(SeasonalBonusPoolRequest item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal bonus pool request.</summary>
public sealed record SeasonalBonusPoolRequestValidator(List<ISeasonalBonusPoolRequestRule> Rules)
{
    public List<string> Validate(SeasonalBonusPoolRequest item)
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

    public void RequireValid(SeasonalBonusPoolRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalBonusPoolRequest> items)
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

/// <summary>Functions of seasonal bonus pool request rules.</summary>
public static class SeasonalBonusPoolRequestRules
{
    public static SeasonalBonusPoolRequestValidator DefaultSeasonalBonusPoolRequestValidator()
    {
        return new SeasonalBonusPoolRequestValidator(
            new List<ISeasonalBonusPoolRequestRule>
            {
                new SeasonalBonusPoolRequestCeilingRangeRule(0.1, 1565.9),
                new SeasonalBonusPoolRequestCodePrefixRule("X"),
                new SeasonalBonusPoolRequestScoreRangeRule(7.4, 863.0),
                new SeasonalBonusPoolRequestRateRangeRule(2.6, 2788.9),
            });
    }
}
