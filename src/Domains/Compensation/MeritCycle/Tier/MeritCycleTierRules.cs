namespace Acme.Workforce.Compensation.MeritCycle.Tier;

/// <summary>A rule a merit cycle tier must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleTierRule
{
    List<string> Check(MeritCycleTier item);
}

public sealed record MeritCycleTierTitleLengthRule(int MaxLength) : IMeritCycleTierRule
{
    public List<string> Check(MeritCycleTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleTierCodeLengthRule(int MaxLength) : IMeritCycleTierRule
{
    public List<string> Check(MeritCycleTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleTierWeightRangeRule(double Min, double Max) : IMeritCycleTierRule
{
    public List<string> Check(MeritCycleTier item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle tier.</summary>
public sealed record MeritCycleTierValidator(List<IMeritCycleTierRule> Rules)
{
    public List<string> Validate(MeritCycleTier item)
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

    public void RequireValid(MeritCycleTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleTier> items)
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

/// <summary>Functions of merit cycle tier rules.</summary>
public static class MeritCycleTierRules
{
    public static MeritCycleTierValidator DefaultMeritCycleTierValidator()
    {
        return new MeritCycleTierValidator(
            new List<IMeritCycleTierRule>
            {
                new MeritCycleTierTitleLengthRule(32),
                new MeritCycleTierCodeLengthRule(3600),
                new MeritCycleTierWeightRangeRule(8.1, 1792.9),
            });
    }
}
