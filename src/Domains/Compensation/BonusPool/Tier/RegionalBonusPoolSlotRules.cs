namespace Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>A rule a regional bonus pool slot must pass. Check returns its problems, or none.</summary>
public interface IRegionalBonusPoolSlotRule
{
    List<string> Check(RegionalBonusPoolSlot item);
}

public sealed record RegionalBonusPoolSlotRequiredCodeRule(string Message) : IRegionalBonusPoolSlotRule
{
    public List<string> Check(RegionalBonusPoolSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalBonusPoolSlotMinimumRangeRule(double Min, double Max) : IRegionalBonusPoolSlotRule
{
    public List<string> Check(RegionalBonusPoolSlot item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalBonusPoolSlotExpiresAtRangeRule(int Min, int Max) : IRegionalBonusPoolSlotRule
{
    public List<string> Check(RegionalBonusPoolSlot item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional bonus pool slot.</summary>
public sealed record RegionalBonusPoolSlotValidator(List<IRegionalBonusPoolSlotRule> Rules)
{
    public List<string> Validate(RegionalBonusPoolSlot item)
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

    public void RequireValid(RegionalBonusPoolSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalBonusPoolSlot> items)
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

/// <summary>Functions of regional bonus pool slot rules.</summary>
public static class RegionalBonusPoolSlotRules
{
    public static RegionalBonusPoolSlotValidator DefaultRegionalBonusPoolSlotValidator()
    {
        return new RegionalBonusPoolSlotValidator(
            new List<IRegionalBonusPoolSlotRule>
            {
                new RegionalBonusPoolSlotRequiredCodeRule("regional bonus pool slot needs a value"),
                new RegionalBonusPoolSlotMinimumRangeRule(1.0, 694.4),
                new RegionalBonusPoolSlotExpiresAtRangeRule(80, 120),
            });
    }
}
