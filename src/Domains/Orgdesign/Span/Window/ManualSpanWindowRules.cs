namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual span window must pass. Check returns its problems, or none.</summary>
public interface IManualSpanWindowRule
{
    List<string> Check(ManualSpanWindow item);
}

public sealed record ManualSpanWindowAllowedCodeRule(List<string> Allowed) : IManualSpanWindowRule
{
    public List<string> Check(ManualSpanWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record ManualSpanWindowGraceDaysRangeRule(int Min, int Max) : IManualSpanWindowRule
{
    public List<string> Check(ManualSpanWindow item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualSpanWindowCodeLengthRule(int MaxLength) : IManualSpanWindowRule
{
    public List<string> Check(ManualSpanWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualSpanWindowEmployeeIdLengthRule(int MaxLength) : IManualSpanWindowRule
{
    public List<string> Check(ManualSpanWindow item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualSpanWindowRequiredCodeRule(string Message) : IManualSpanWindowRule
{
    public List<string> Check(ManualSpanWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualSpanWindowCodePrefixRule(string Prefix) : IManualSpanWindowRule
{
    public List<string> Check(ManualSpanWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual span window.</summary>
public sealed record ManualSpanWindowValidator(List<IManualSpanWindowRule> Rules)
{
    public List<string> Validate(ManualSpanWindow item)
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

    public void RequireValid(ManualSpanWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualSpanWindow> items)
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

/// <summary>Functions of manual span window rules.</summary>
public static class ManualSpanWindowRules
{
    public static ManualSpanWindowValidator DefaultManualSpanWindowValidator()
    {
        return new ManualSpanWindowValidator(
            new List<IManualSpanWindowRule>
            {
                new ManualSpanWindowAllowedCodeRule(new List<string>()),
                new ManualSpanWindowGraceDaysRangeRule(32, 3600),
                new ManualSpanWindowCodeLengthRule(1000),
                new ManualSpanWindowEmployeeIdLengthRule(80),
                new ManualSpanWindowRequiredCodeRule("manual span window needs an entry"),
                new ManualSpanWindowCodePrefixRule("X"),
            });
    }
}
