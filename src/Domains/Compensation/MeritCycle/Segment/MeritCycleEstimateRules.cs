namespace Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>A rule a merit cycle estimate must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleEstimateRule
{
    List<string> Check(MeritCycleEstimate item);
}

public sealed record MeritCycleEstimateCodeLengthRule(int MaxLength) : IMeritCycleEstimateRule
{
    public List<string> Check(MeritCycleEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleEstimateRequiredCodeRule(string Message) : IMeritCycleEstimateRule
{
    public List<string> Check(MeritCycleEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleEstimateRequiredChannelRule(string Message) : IMeritCycleEstimateRule
{
    public List<string> Check(MeritCycleEstimate item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle estimate.</summary>
public sealed record MeritCycleEstimateValidator(List<IMeritCycleEstimateRule> Rules)
{
    public List<string> Validate(MeritCycleEstimate item)
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

    public void RequireValid(MeritCycleEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleEstimate> items)
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

/// <summary>Functions of merit cycle estimate rules.</summary>
public static class MeritCycleEstimateRules
{
    public static MeritCycleEstimateValidator DefaultMeritCycleEstimateValidator()
    {
        return new MeritCycleEstimateValidator(
            new List<IMeritCycleEstimateRule>
            {
                new MeritCycleEstimateCodeLengthRule(80),
                new MeritCycleEstimateRequiredCodeRule("merit cycle estimate needs this field"),
                new MeritCycleEstimateRequiredChannelRule("merit cycle estimate needs an entry"),
            });
    }
}
