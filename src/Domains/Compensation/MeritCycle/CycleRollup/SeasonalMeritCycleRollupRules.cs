namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal merit cycle rollup must pass. Check returns its problems, or none.</summary>
public interface ISeasonalMeritCycleRollupRule
{
    List<string> Check(SeasonalMeritCycleRollup item);
}

public sealed record SeasonalMeritCycleRollupAllowedCodeRule(List<string> Allowed) : ISeasonalMeritCycleRollupRule
{
    public List<string> Check(SeasonalMeritCycleRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMeritCycleRollupRequiredTitleRule(string Message) : ISeasonalMeritCycleRollupRule
{
    public List<string> Check(SeasonalMeritCycleRollup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMeritCycleRollupWeightRangeRule(double Min, double Max) : ISeasonalMeritCycleRollupRule
{
    public List<string> Check(SeasonalMeritCycleRollup item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal merit cycle rollup.</summary>
public sealed record SeasonalMeritCycleRollupValidator(List<ISeasonalMeritCycleRollupRule> Rules)
{
    public List<string> Validate(SeasonalMeritCycleRollup item)
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

    public void RequireValid(SeasonalMeritCycleRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalMeritCycleRollup> items)
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

/// <summary>Functions of seasonal merit cycle rollup rules.</summary>
public static class SeasonalMeritCycleRollupRules
{
    public static SeasonalMeritCycleRollupValidator DefaultSeasonalMeritCycleRollupValidator()
    {
        return new SeasonalMeritCycleRollupValidator(
            new List<ISeasonalMeritCycleRollupRule>
            {
                new SeasonalMeritCycleRollupAllowedCodeRule(new List<string> { "capped", "primary", "fallback" }),
                new SeasonalMeritCycleRollupRequiredTitleRule("seasonal merit cycle rollup needs an entry"),
                new SeasonalMeritCycleRollupWeightRangeRule(3.3, 380.1),
            });
    }
}
