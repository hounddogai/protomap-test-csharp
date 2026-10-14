namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a merit cycle exception must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleExceptionRule
{
    List<string> Check(MeritCycleException item);
}

public sealed record MeritCycleExceptionCodePrefixRule(string Prefix) : IMeritCycleExceptionRule
{
    public List<string> Check(MeritCycleException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleExceptionRequiredTitleRule(string Message) : IMeritCycleExceptionRule
{
    public List<string> Check(MeritCycleException item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleExceptionStartsAtRangeRule(int Min, int Max) : IMeritCycleExceptionRule
{
    public List<string> Check(MeritCycleException item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MeritCycleExceptionRequiredCodeRule(string Message) : IMeritCycleExceptionRule
{
    public List<string> Check(MeritCycleException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleExceptionAllowedChecksumRule(List<string> Allowed) : IMeritCycleExceptionRule
{
    public List<string> Check(MeritCycleException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle exception.</summary>
public sealed record MeritCycleExceptionValidator(List<IMeritCycleExceptionRule> Rules)
{
    public List<string> Validate(MeritCycleException item)
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

    public void RequireValid(MeritCycleException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleException> items)
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

/// <summary>Functions of merit cycle exception rules.</summary>
public static class MeritCycleExceptionRules
{
    public static MeritCycleExceptionValidator DefaultMeritCycleExceptionValidator()
    {
        return new MeritCycleExceptionValidator(
            new List<IMeritCycleExceptionRule>
            {
                new MeritCycleExceptionCodePrefixRule("REF"),
                new MeritCycleExceptionRequiredTitleRule("merit cycle exception needs a value"),
                new MeritCycleExceptionStartsAtRangeRule(120, 3600),
                new MeritCycleExceptionRequiredCodeRule("merit cycle exception needs a value"),
                new MeritCycleExceptionAllowedChecksumRule(new List<string> { "manual", "tiered" }),
            });
    }
}
