namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>A rule a rolling pay run cycle must pass. Check returns its problems, or none.</summary>
public interface IRollingPayRunCycleRule
{
    List<string> Check(RollingPayRunCycle item);
}

public sealed record RollingPayRunCycleCodePrefixRule(string Prefix) : IRollingPayRunCycleRule
{
    public List<string> Check(RollingPayRunCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPayRunCycleRequiredExternalRefRule(string Message) : IRollingPayRunCycleRule
{
    public List<string> Check(RollingPayRunCycle item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPayRunCycleExternalRefLengthRule(int MaxLength) : IRollingPayRunCycleRule
{
    public List<string> Check(RollingPayRunCycle item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling pay run cycle.</summary>
public sealed record RollingPayRunCycleValidator(List<IRollingPayRunCycleRule> Rules)
{
    public List<string> Validate(RollingPayRunCycle item)
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

    public void RequireValid(RollingPayRunCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPayRunCycle> items)
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

/// <summary>Functions of rolling pay run cycle rules.</summary>
public static class RollingPayRunCycleRules
{
    public static RollingPayRunCycleValidator DefaultRollingPayRunCycleValidator()
    {
        return new RollingPayRunCycleValidator(
            new List<IRollingPayRunCycleRule>
            {
                new RollingPayRunCycleCodePrefixRule("REF"),
                new RollingPayRunCycleRequiredExternalRefRule("rolling pay run cycle needs an entry"),
                new RollingPayRunCycleExternalRefLengthRule(80),
            });
    }
}
