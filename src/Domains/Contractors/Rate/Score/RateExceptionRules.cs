namespace Acme.Workforce.Contractors.Rate.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rate exception must pass. Check returns its problems, or none.</summary>
public interface IRateExceptionRule
{
    List<string> Check(RateException item);
}

public sealed record RateExceptionRequiredCodeRule(string Message) : IRateExceptionRule
{
    public List<string> Check(RateException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateExceptionAllowedTitleRule(List<string> Allowed) : IRateExceptionRule
{
    public List<string> Check(RateException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record RateExceptionRequiredNotesRule(string Message) : IRateExceptionRule
{
    public List<string> Check(RateException item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateExceptionCodePrefixRule(string Prefix) : IRateExceptionRule
{
    public List<string> Check(RateException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate exception.</summary>
public sealed record RateExceptionValidator(List<IRateExceptionRule> Rules)
{
    public List<string> Validate(RateException item)
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

    public void RequireValid(RateException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateException> items)
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

/// <summary>Functions of rate exception rules.</summary>
public static class RateExceptionRules
{
    public static RateExceptionValidator DefaultRateExceptionValidator()
    {
        return new RateExceptionValidator(
            new List<IRateExceptionRule>
            {
                new RateExceptionRequiredCodeRule("rate exception needs this field"),
                new RateExceptionAllowedTitleRule(new List<string> { "rolling" }),
                new RateExceptionRequiredNotesRule("rate exception needs this field"),
                new RateExceptionCodePrefixRule("ACM"),
            });
    }
}
