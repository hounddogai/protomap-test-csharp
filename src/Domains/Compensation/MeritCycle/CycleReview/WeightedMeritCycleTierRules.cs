namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted merit cycle tier must pass. Check returns its problems, or none.</summary>
public interface IWeightedMeritCycleTierRule
{
    List<string> Check(WeightedMeritCycleTier item);
}

public sealed record WeightedMeritCycleTierRequiredLabelRule(string Message) : IWeightedMeritCycleTierRule
{
    public List<string> Check(WeightedMeritCycleTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedMeritCycleTierAllowedContactEmailRule(List<string> Allowed) : IWeightedMeritCycleTierRule
{
    public List<string> Check(WeightedMeritCycleTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is stale" };
        }
        return new List<string>();
    }
}

public sealed record WeightedMeritCycleTierRequiredCodeRule(string Message) : IWeightedMeritCycleTierRule
{
    public List<string> Check(WeightedMeritCycleTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted merit cycle tier.</summary>
public sealed record WeightedMeritCycleTierValidator(List<IWeightedMeritCycleTierRule> Rules)
{
    public List<string> Validate(WeightedMeritCycleTier item)
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

    public void RequireValid(WeightedMeritCycleTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedMeritCycleTier> items)
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

/// <summary>Functions of weighted merit cycle tier rules.</summary>
public static class WeightedMeritCycleTierRules
{
    public static WeightedMeritCycleTierValidator DefaultWeightedMeritCycleTierValidator()
    {
        return new WeightedMeritCycleTierValidator(
            new List<IWeightedMeritCycleTierRule>
            {
                new WeightedMeritCycleTierRequiredLabelRule("weighted merit cycle tier needs a value"),
                new WeightedMeritCycleTierAllowedContactEmailRule(new List<string>()),
                new WeightedMeritCycleTierRequiredCodeRule("weighted merit cycle tier needs this field"),
            });
    }
}
