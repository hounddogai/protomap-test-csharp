namespace Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>A rule a merit cycle threshold must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleThresholdRule
{
    List<string> Check(MeritCycleThreshold item);
}

public sealed record MeritCycleThresholdRequiredNameRule(string Message) : IMeritCycleThresholdRule
{
    public List<string> Check(MeritCycleThreshold item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleThresholdWeightRangeRule(double Min, double Max) : IMeritCycleThresholdRule
{
    public List<string> Check(MeritCycleThreshold item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleThresholdRequiredLocaleRule(string Message) : IMeritCycleThresholdRule
{
    public List<string> Check(MeritCycleThreshold item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle threshold.</summary>
public sealed record MeritCycleThresholdValidator(List<IMeritCycleThresholdRule> Rules)
{
    public List<string> Validate(MeritCycleThreshold item)
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

    public void RequireValid(MeritCycleThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleThreshold> items)
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

/// <summary>Functions of merit cycle threshold rules.</summary>
public static class MeritCycleThresholdRules
{
    public static MeritCycleThresholdValidator DefaultMeritCycleThresholdValidator()
    {
        return new MeritCycleThresholdValidator(
            new List<IMeritCycleThresholdRule>
            {
                new MeritCycleThresholdRequiredNameRule("merit cycle threshold needs a value"),
                new MeritCycleThresholdWeightRangeRule(9.7, 1969.1),
                new MeritCycleThresholdRequiredLocaleRule("merit cycle threshold needs a value"),
            });
    }
}
