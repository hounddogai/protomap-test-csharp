namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped nomination window must pass. Check returns its problems, or none.</summary>
public interface ICappedNominationWindowRule
{
    List<string> Check(CappedNominationWindow item);
}

public sealed record CappedNominationWindowNameLengthRule(int MaxLength) : ICappedNominationWindowRule
{
    public List<string> Check(CappedNominationWindow item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedNominationWindowLabelLengthRule(int MaxLength) : ICappedNominationWindowRule
{
    public List<string> Check(CappedNominationWindow item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedNominationWindowAllowedLabelRule(List<string> Allowed) : ICappedNominationWindowRule
{
    public List<string> Check(CappedNominationWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record CappedNominationWindowCodePrefixRule(string Prefix) : ICappedNominationWindowRule
{
    public List<string> Check(CappedNominationWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedNominationWindowCodeLengthRule(int MaxLength) : ICappedNominationWindowRule
{
    public List<string> Check(CappedNominationWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped nomination window.</summary>
public sealed record CappedNominationWindowValidator(List<ICappedNominationWindowRule> Rules)
{
    public List<string> Validate(CappedNominationWindow item)
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

    public void RequireValid(CappedNominationWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedNominationWindow> items)
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

/// <summary>Functions of capped nomination window rules.</summary>
public static class CappedNominationWindowRules
{
    public static CappedNominationWindowValidator DefaultCappedNominationWindowValidator()
    {
        return new CappedNominationWindowValidator(
            new List<ICappedNominationWindowRule>
            {
                new CappedNominationWindowNameLengthRule(86400),
                new CappedNominationWindowLabelLengthRule(255),
                new CappedNominationWindowAllowedLabelRule(new List<string> { "tiered", "fallback" }),
                new CappedNominationWindowCodePrefixRule("REF"),
                new CappedNominationWindowCodeLengthRule(255),
            });
    }
}
