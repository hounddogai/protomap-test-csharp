namespace Acme.Workforce.Orgdesign.CostModel.Group;

/// <summary>A rule a global cost model quota must pass. Check returns its problems, or none.</summary>
public interface IGlobalCostModelQuotaRule
{
    List<string> Check(GlobalCostModelQuota item);
}

public sealed record GlobalCostModelQuotaEndsAtRangeRule(int Min, int Max) : IGlobalCostModelQuotaRule
{
    public List<string> Check(GlobalCostModelQuota item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalCostModelQuotaRatioRangeRule(double Min, double Max) : IGlobalCostModelQuotaRule
{
    public List<string> Check(GlobalCostModelQuota item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalCostModelQuotaScoreRangeRule(double Min, double Max) : IGlobalCostModelQuotaRule
{
    public List<string> Check(GlobalCostModelQuota item)
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

public sealed record GlobalCostModelQuotaRateRangeRule(double Min, double Max) : IGlobalCostModelQuotaRule
{
    public List<string> Check(GlobalCostModelQuota item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalCostModelQuotaRequiredNameRule(string Message) : IGlobalCostModelQuotaRule
{
    public List<string> Check(GlobalCostModelQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global cost model quota.</summary>
public sealed record GlobalCostModelQuotaValidator(List<IGlobalCostModelQuotaRule> Rules)
{
    public List<string> Validate(GlobalCostModelQuota item)
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

    public void RequireValid(GlobalCostModelQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalCostModelQuota> items)
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

/// <summary>Functions of global cost model quota rules.</summary>
public static class GlobalCostModelQuotaRules
{
    public static GlobalCostModelQuotaValidator DefaultGlobalCostModelQuotaValidator()
    {
        return new GlobalCostModelQuotaValidator(
            new List<IGlobalCostModelQuotaRule>
            {
                new GlobalCostModelQuotaEndsAtRangeRule(80, 1000),
                new GlobalCostModelQuotaRatioRangeRule(9.9, 596.2),
                new GlobalCostModelQuotaScoreRangeRule(5.5, 427.4),
                new GlobalCostModelQuotaRateRangeRule(6.9, 2449.7),
                new GlobalCostModelQuotaRequiredNameRule("global cost model quota needs a value"),
            });
    }
}
