namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual window must pass. Check returns its problems, or none.</summary>
public interface IAccrualWindowRule
{
    List<string> Check(AccrualWindow item);
}

public sealed record AccrualWindowAllowedTitleRule(List<string> Allowed) : IAccrualWindowRule
{
    public List<string> Check(AccrualWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record AccrualWindowLocaleLengthRule(int MaxLength) : IAccrualWindowRule
{
    public List<string> Check(AccrualWindow item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualWindowCodePrefixRule(string Prefix) : IAccrualWindowRule
{
    public List<string> Check(AccrualWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualWindowRequiredLocaleRule(string Message) : IAccrualWindowRule
{
    public List<string> Check(AccrualWindow item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualWindowRequiredTitleRule(string Message) : IAccrualWindowRule
{
    public List<string> Check(AccrualWindow item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualWindowRequiredCodeRule(string Message) : IAccrualWindowRule
{
    public List<string> Check(AccrualWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual window.</summary>
public sealed record AccrualWindowValidator(List<IAccrualWindowRule> Rules)
{
    public List<string> Validate(AccrualWindow item)
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

    public void RequireValid(AccrualWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualWindow> items)
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

/// <summary>Functions of accrual window rules.</summary>
public static class AccrualWindowRules
{
    public static AccrualWindowValidator DefaultAccrualWindowValidator()
    {
        return new AccrualWindowValidator(
            new List<IAccrualWindowRule>
            {
                new AccrualWindowAllowedTitleRule(new List<string> { "seasonal", "strict", "manual" }),
                new AccrualWindowLocaleLengthRule(255),
                new AccrualWindowCodePrefixRule("Q"),
                new AccrualWindowRequiredLocaleRule("accrual window needs this field"),
                new AccrualWindowRequiredTitleRule("accrual window needs this field"),
                new AccrualWindowRequiredCodeRule("accrual window needs a value"),
            });
    }
}
