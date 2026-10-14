namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling pay band estimate must pass. Check returns its problems, or none.</summary>
public interface IRollingPayBandEstimateRule
{
    List<string> Check(RollingPayBandEstimate item);
}

public sealed record RollingPayBandEstimateCreatedAtRangeRule(int Min, int Max) : IRollingPayBandEstimateRule
{
    public List<string> Check(RollingPayBandEstimate item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingPayBandEstimateAllowedCodeRule(List<string> Allowed) : IRollingPayBandEstimateRule
{
    public List<string> Check(RollingPayBandEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record RollingPayBandEstimateWeightRangeRule(double Min, double Max) : IRollingPayBandEstimateRule
{
    public List<string> Check(RollingPayBandEstimate item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPayBandEstimateScoreRangeRule(double Min, double Max) : IRollingPayBandEstimateRule
{
    public List<string> Check(RollingPayBandEstimate item)
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

public sealed record RollingPayBandEstimateCodePrefixRule(string Prefix) : IRollingPayBandEstimateRule
{
    public List<string> Check(RollingPayBandEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPayBandEstimateRequiredCodeRule(string Message) : IRollingPayBandEstimateRule
{
    public List<string> Check(RollingPayBandEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling pay band estimate.</summary>
public sealed record RollingPayBandEstimateValidator(List<IRollingPayBandEstimateRule> Rules)
{
    public List<string> Validate(RollingPayBandEstimate item)
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

    public void RequireValid(RollingPayBandEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPayBandEstimate> items)
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

/// <summary>Functions of rolling pay band estimate rules.</summary>
public static class RollingPayBandEstimateRules
{
    public static RollingPayBandEstimateValidator DefaultRollingPayBandEstimateValidator()
    {
        return new RollingPayBandEstimateValidator(
            new List<IRollingPayBandEstimateRule>
            {
                new RollingPayBandEstimateCreatedAtRangeRule(32, 3600),
                new RollingPayBandEstimateAllowedCodeRule(new List<string>()),
                new RollingPayBandEstimateWeightRangeRule(3.0, 949.9),
                new RollingPayBandEstimateScoreRangeRule(1.0, 1470.1),
                new RollingPayBandEstimateCodePrefixRule("REF"),
                new RollingPayBandEstimateRequiredCodeRule("rolling pay band estimate needs this field"),
            });
    }
}
