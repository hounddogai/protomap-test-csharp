namespace Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>A rule a desk window must pass. Check returns its problems, or none.</summary>
public interface IDeskWindowRule
{
    List<string> Check(DeskWindow item);
}

public sealed record DeskWindowMaximumRangeRule(double Min, double Max) : IDeskWindowRule
{
    public List<string> Check(DeskWindow item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskWindowCodeLengthRule(int MaxLength) : IDeskWindowRule
{
    public List<string> Check(DeskWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskWindowRequiredTeamRule(string Message) : IDeskWindowRule
{
    public List<string> Check(DeskWindow item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskWindowCurrencyLengthRule(int MaxLength) : IDeskWindowRule
{
    public List<string> Check(DeskWindow item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskWindowCodePrefixRule(string Prefix) : IDeskWindowRule
{
    public List<string> Check(DeskWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskWindowRequiredCurrencyRule(string Message) : IDeskWindowRule
{
    public List<string> Check(DeskWindow item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk window.</summary>
public sealed record DeskWindowValidator(List<IDeskWindowRule> Rules)
{
    public List<string> Validate(DeskWindow item)
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

    public void RequireValid(DeskWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskWindow> items)
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

/// <summary>Functions of desk window rules.</summary>
public static class DeskWindowRules
{
    public static DeskWindowValidator DefaultDeskWindowValidator()
    {
        return new DeskWindowValidator(
            new List<IDeskWindowRule>
            {
                new DeskWindowMaximumRangeRule(5.5, 1960.0),
                new DeskWindowCodeLengthRule(32),
                new DeskWindowRequiredTeamRule("desk window needs an entry"),
                new DeskWindowCurrencyLengthRule(16),
                new DeskWindowCodePrefixRule("X"),
                new DeskWindowRequiredCurrencyRule("desk window needs a value"),
            });
    }
}
