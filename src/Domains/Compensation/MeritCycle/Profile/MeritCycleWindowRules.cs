namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a merit cycle window must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleWindowRule
{
    List<string> Check(MeritCycleWindow item);
}

public sealed record MeritCycleWindowAllowedCodeRule(List<string> Allowed) : IMeritCycleWindowRule
{
    public List<string> Check(MeritCycleWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleWindowCodePrefixRule(string Prefix) : IMeritCycleWindowRule
{
    public List<string> Check(MeritCycleWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleWindowCodeLengthRule(int MaxLength) : IMeritCycleWindowRule
{
    public List<string> Check(MeritCycleWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleWindowArchivedGuardRule(double Limit) : IMeritCycleWindowRule
{
    public List<string> Check(MeritCycleWindow item)
    {
        if (item.Archived && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle window.</summary>
public sealed record MeritCycleWindowValidator(List<IMeritCycleWindowRule> Rules)
{
    public List<string> Validate(MeritCycleWindow item)
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

    public void RequireValid(MeritCycleWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleWindow> items)
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

/// <summary>Functions of merit cycle window rules.</summary>
public static class MeritCycleWindowRules
{
    public static MeritCycleWindowValidator DefaultMeritCycleWindowValidator()
    {
        return new MeritCycleWindowValidator(
            new List<IMeritCycleWindowRule>
            {
                new MeritCycleWindowAllowedCodeRule(new List<string>()),
                new MeritCycleWindowCodePrefixRule("X"),
                new MeritCycleWindowCodeLengthRule(64),
                new MeritCycleWindowArchivedGuardRule(9.2),
            });
    }
}
