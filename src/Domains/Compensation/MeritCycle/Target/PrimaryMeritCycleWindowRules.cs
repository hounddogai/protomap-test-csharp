namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary merit cycle window must pass. Check returns its problems, or none.</summary>
public interface IPrimaryMeritCycleWindowRule
{
    List<string> Check(PrimaryMeritCycleWindow item);
}

public sealed record PrimaryMeritCycleWindowRequiredNameRule(string Message) : IPrimaryMeritCycleWindowRule
{
    public List<string> Check(PrimaryMeritCycleWindow item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMeritCycleWindowAllowedNotesRule(List<string> Allowed) : IPrimaryMeritCycleWindowRule
{
    public List<string> Check(PrimaryMeritCycleWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMeritCycleWindowCodePrefixRule(string Prefix) : IPrimaryMeritCycleWindowRule
{
    public List<string> Check(PrimaryMeritCycleWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMeritCycleWindowCodeLengthRule(int MaxLength) : IPrimaryMeritCycleWindowRule
{
    public List<string> Check(PrimaryMeritCycleWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary merit cycle window.</summary>
public sealed record PrimaryMeritCycleWindowValidator(List<IPrimaryMeritCycleWindowRule> Rules)
{
    public List<string> Validate(PrimaryMeritCycleWindow item)
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

    public void RequireValid(PrimaryMeritCycleWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryMeritCycleWindow> items)
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

/// <summary>Functions of primary merit cycle window rules.</summary>
public static class PrimaryMeritCycleWindowRules
{
    public static PrimaryMeritCycleWindowValidator DefaultPrimaryMeritCycleWindowValidator()
    {
        return new PrimaryMeritCycleWindowValidator(
            new List<IPrimaryMeritCycleWindowRule>
            {
                new PrimaryMeritCycleWindowRequiredNameRule("primary merit cycle window needs an entry"),
                new PrimaryMeritCycleWindowAllowedNotesRule(new List<string> { "batched", "global" }),
                new PrimaryMeritCycleWindowCodePrefixRule("ACM"),
                new PrimaryMeritCycleWindowCodeLengthRule(86400),
            });
    }
}
