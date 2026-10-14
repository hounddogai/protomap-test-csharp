namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>A rule a primary merit cycle rollup must pass. Check returns its problems, or none.</summary>
public interface IPrimaryMeritCycleRollupRule
{
    List<string> Check(PrimaryMeritCycleRollup item);
}

public sealed record PrimaryMeritCycleRollupGraceDaysRangeRule(int Min, int Max) : IPrimaryMeritCycleRollupRule
{
    public List<string> Check(PrimaryMeritCycleRollup item)
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

public sealed record PrimaryMeritCycleRollupCodePrefixRule(string Prefix) : IPrimaryMeritCycleRollupRule
{
    public List<string> Check(PrimaryMeritCycleRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMeritCycleRollupExpiresAtRangeRule(int Min, int Max) : IPrimaryMeritCycleRollupRule
{
    public List<string> Check(PrimaryMeritCycleRollup item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary merit cycle rollup.</summary>
public sealed record PrimaryMeritCycleRollupValidator(List<IPrimaryMeritCycleRollupRule> Rules)
{
    public List<string> Validate(PrimaryMeritCycleRollup item)
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

    public void RequireValid(PrimaryMeritCycleRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryMeritCycleRollup> items)
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

/// <summary>Functions of primary merit cycle rollup rules.</summary>
public static class PrimaryMeritCycleRollupRules
{
    public static PrimaryMeritCycleRollupValidator DefaultPrimaryMeritCycleRollupValidator()
    {
        return new PrimaryMeritCycleRollupValidator(
            new List<IPrimaryMeritCycleRollupRule>
            {
                new PrimaryMeritCycleRollupGraceDaysRangeRule(120, 3600),
                new PrimaryMeritCycleRollupCodePrefixRule("ACM"),
                new PrimaryMeritCycleRollupExpiresAtRangeRule(16, 80),
            });
    }
}
