namespace Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>A rule a tiered pay band estimate must pass. Check returns its problems, or none.</summary>
public interface ITieredPayBandEstimateRule
{
    List<string> Check(TieredPayBandEstimate item);
}

public sealed record TieredPayBandEstimateRequiredSkillRule(string Message) : ITieredPayBandEstimateRule
{
    public List<string> Check(TieredPayBandEstimate item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPayBandEstimateRequiredLabelRule(string Message) : ITieredPayBandEstimateRule
{
    public List<string> Check(TieredPayBandEstimate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPayBandEstimateCapacityRangeRule(int Min, int Max) : ITieredPayBandEstimateRule
{
    public List<string> Check(TieredPayBandEstimate item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPayBandEstimateRequiredCodeRule(string Message) : ITieredPayBandEstimateRule
{
    public List<string> Check(TieredPayBandEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered pay band estimate.</summary>
public sealed record TieredPayBandEstimateValidator(List<ITieredPayBandEstimateRule> Rules)
{
    public List<string> Validate(TieredPayBandEstimate item)
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

    public void RequireValid(TieredPayBandEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPayBandEstimate> items)
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

/// <summary>Functions of tiered pay band estimate rules.</summary>
public static class TieredPayBandEstimateRules
{
    public static TieredPayBandEstimateValidator DefaultTieredPayBandEstimateValidator()
    {
        return new TieredPayBandEstimateValidator(
            new List<ITieredPayBandEstimateRule>
            {
                new TieredPayBandEstimateRequiredSkillRule("tiered pay band estimate needs an entry"),
                new TieredPayBandEstimateRequiredLabelRule("tiered pay band estimate needs an entry"),
                new TieredPayBandEstimateCapacityRangeRule(1000, 1000),
                new TieredPayBandEstimateRequiredCodeRule("tiered pay band estimate needs this field"),
            });
    }
}
