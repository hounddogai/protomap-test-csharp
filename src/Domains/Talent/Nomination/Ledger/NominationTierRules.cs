namespace Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>A rule a nomination tier must pass. Check returns its problems, or none.</summary>
public interface INominationTierRule
{
    List<string> Check(NominationTier item);
}

public sealed record NominationTierRequiredCodeRule(string Message) : INominationTierRule
{
    public List<string> Check(NominationTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationTierCapacityRangeRule(int Min, int Max) : INominationTierRule
{
    public List<string> Check(NominationTier item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record NominationTierScoreRangeRule(double Min, double Max) : INominationTierRule
{
    public List<string> Check(NominationTier item)
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

/// <summary>Runs every rule of a nomination tier.</summary>
public sealed record NominationTierValidator(List<INominationTierRule> Rules)
{
    public List<string> Validate(NominationTier item)
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

    public void RequireValid(NominationTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationTier> items)
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

/// <summary>Functions of nomination tier rules.</summary>
public static class NominationTierRules
{
    public static NominationTierValidator DefaultNominationTierValidator()
    {
        return new NominationTierValidator(
            new List<INominationTierRule>
            {
                new NominationTierRequiredCodeRule("nomination tier needs an entry"),
                new NominationTierCapacityRangeRule(255, 1000),
                new NominationTierScoreRangeRule(2.4, 2317.6),
            });
    }
}
