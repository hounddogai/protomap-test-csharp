namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>A rule a merit cycle baseline must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleBaselineRule
{
    List<string> Check(MeritCycleBaseline item);
}

public sealed record MeritCycleBaselineCodePrefixRule(string Prefix) : IMeritCycleBaselineRule
{
    public List<string> Check(MeritCycleBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleBaselineCodeLengthRule(int MaxLength) : IMeritCycleBaselineRule
{
    public List<string> Check(MeritCycleBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleBaselineRequiredNameRule(string Message) : IMeritCycleBaselineRule
{
    public List<string> Check(MeritCycleBaseline item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle baseline.</summary>
public sealed record MeritCycleBaselineValidator(List<IMeritCycleBaselineRule> Rules)
{
    public List<string> Validate(MeritCycleBaseline item)
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

    public void RequireValid(MeritCycleBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleBaseline> items)
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

/// <summary>Functions of merit cycle baseline rules.</summary>
public static class MeritCycleBaselineRules
{
    public static MeritCycleBaselineValidator DefaultMeritCycleBaselineValidator()
    {
        return new MeritCycleBaselineValidator(
            new List<IMeritCycleBaselineRule>
            {
                new MeritCycleBaselineCodePrefixRule("X"),
                new MeritCycleBaselineCodeLengthRule(32),
                new MeritCycleBaselineRequiredNameRule("merit cycle baseline needs an entry"),
            });
    }
}
