namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>A rule a lenient merit cycle window must pass. Check returns its problems, or none.</summary>
public interface ILenientMeritCycleWindowRule
{
    List<string> Check(LenientMeritCycleWindow item);
}

public sealed record LenientMeritCycleWindowCodePrefixRule(string Prefix) : ILenientMeritCycleWindowRule
{
    public List<string> Check(LenientMeritCycleWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientMeritCycleWindowOwnerIdLengthRule(int MaxLength) : ILenientMeritCycleWindowRule
{
    public List<string> Check(LenientMeritCycleWindow item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientMeritCycleWindowSummaryLengthRule(int MaxLength) : ILenientMeritCycleWindowRule
{
    public List<string> Check(LenientMeritCycleWindow item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient merit cycle window.</summary>
public sealed record LenientMeritCycleWindowValidator(List<ILenientMeritCycleWindowRule> Rules)
{
    public List<string> Validate(LenientMeritCycleWindow item)
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

    public void RequireValid(LenientMeritCycleWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientMeritCycleWindow> items)
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

/// <summary>Functions of lenient merit cycle window rules.</summary>
public static class LenientMeritCycleWindowRules
{
    public static LenientMeritCycleWindowValidator DefaultLenientMeritCycleWindowValidator()
    {
        return new LenientMeritCycleWindowValidator(
            new List<ILenientMeritCycleWindowRule>
            {
                new LenientMeritCycleWindowCodePrefixRule("X"),
                new LenientMeritCycleWindowOwnerIdLengthRule(86400),
                new LenientMeritCycleWindowSummaryLengthRule(1000),
            });
    }
}
