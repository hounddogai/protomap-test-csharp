namespace Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>A rule a merit cycle rollup must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleRollupRule
{
    List<string> Check(MeritCycleRollup item);
}

public sealed record MeritCycleRollupCurrencyLengthRule(int MaxLength) : IMeritCycleRollupRule
{
    public List<string> Check(MeritCycleRollup item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleRollupFteRangeRule(double Min, double Max) : IMeritCycleRollupRule
{
    public List<string> Check(MeritCycleRollup item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleRollupTitleLengthRule(int MaxLength) : IMeritCycleRollupRule
{
    public List<string> Check(MeritCycleRollup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle rollup.</summary>
public sealed record MeritCycleRollupValidator(List<IMeritCycleRollupRule> Rules)
{
    public List<string> Validate(MeritCycleRollup item)
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

    public void RequireValid(MeritCycleRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleRollup> items)
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

/// <summary>Functions of merit cycle rollup rules.</summary>
public static class MeritCycleRollupRules
{
    public static MeritCycleRollupValidator DefaultMeritCycleRollupValidator()
    {
        return new MeritCycleRollupValidator(
            new List<IMeritCycleRollupRule>
            {
                new MeritCycleRollupCurrencyLengthRule(64),
                new MeritCycleRollupFteRangeRule(2.0, 434.7),
                new MeritCycleRollupTitleLengthRule(80),
            });
    }
}
