namespace Acme.Workforce.Orgdesign.Headcount.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount window must pass. Check returns its problems, or none.</summary>
public interface IHeadcountWindowRule
{
    List<string> Check(HeadcountWindow item);
}

public sealed record HeadcountWindowRequiredSourceRule(string Message) : IHeadcountWindowRule
{
    public List<string> Check(HeadcountWindow item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountWindowRequiredCodeRule(string Message) : IHeadcountWindowRule
{
    public List<string> Check(HeadcountWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountWindowCodePrefixRule(string Prefix) : IHeadcountWindowRule
{
    public List<string> Check(HeadcountWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountWindowLocationLengthRule(int MaxLength) : IHeadcountWindowRule
{
    public List<string> Check(HeadcountWindow item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountWindowLeaveDaysRangeRule(int Min, int Max) : IHeadcountWindowRule
{
    public List<string> Check(HeadcountWindow item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountWindowAllowedLabelRule(List<string> Allowed) : IHeadcountWindowRule
{
    public List<string> Check(HeadcountWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount window.</summary>
public sealed record HeadcountWindowValidator(List<IHeadcountWindowRule> Rules)
{
    public List<string> Validate(HeadcountWindow item)
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

    public void RequireValid(HeadcountWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountWindow> items)
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

/// <summary>Functions of headcount window rules.</summary>
public static class HeadcountWindowRules
{
    public static HeadcountWindowValidator DefaultHeadcountWindowValidator()
    {
        return new HeadcountWindowValidator(
            new List<IHeadcountWindowRule>
            {
                new HeadcountWindowRequiredSourceRule("headcount window needs this field"),
                new HeadcountWindowRequiredCodeRule("headcount window needs an entry"),
                new HeadcountWindowCodePrefixRule("REF"),
                new HeadcountWindowLocationLengthRule(86400),
                new HeadcountWindowLeaveDaysRangeRule(32, 1000),
                new HeadcountWindowAllowedLabelRule(new List<string> { "seasonal", "legacy" }),
            });
    }
}
