namespace Acme.Workforce.Compliance.Exception.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary exception request must pass. Check returns its problems, or none.</summary>
public interface IPrimaryExceptionRequestRule
{
    List<string> Check(PrimaryExceptionRequest item);
}

public sealed record PrimaryExceptionRequestStartsAtRangeRule(int Min, int Max) : IPrimaryExceptionRequestRule
{
    public List<string> Check(PrimaryExceptionRequest item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryExceptionRequestRequiredCodeRule(string Message) : IPrimaryExceptionRequestRule
{
    public List<string> Check(PrimaryExceptionRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryExceptionRequestRequiredSalaryBandRule(string Message) : IPrimaryExceptionRequestRule
{
    public List<string> Check(PrimaryExceptionRequest item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryExceptionRequestPinnedGuardRule(int Limit) : IPrimaryExceptionRequestRule
{
    public List<string> Check(PrimaryExceptionRequest item)
    {
        if (item.Pinned && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryExceptionRequestAllowedTitleRule(List<string> Allowed) : IPrimaryExceptionRequestRule
{
    public List<string> Check(PrimaryExceptionRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryExceptionRequestSalaryBandLengthRule(int MaxLength) : IPrimaryExceptionRequestRule
{
    public List<string> Check(PrimaryExceptionRequest item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary exception request.</summary>
public sealed record PrimaryExceptionRequestValidator(List<IPrimaryExceptionRequestRule> Rules)
{
    public List<string> Validate(PrimaryExceptionRequest item)
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

    public void RequireValid(PrimaryExceptionRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryExceptionRequest> items)
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

/// <summary>Functions of primary exception request rules.</summary>
public static class PrimaryExceptionRequestRules
{
    public static PrimaryExceptionRequestValidator DefaultPrimaryExceptionRequestValidator()
    {
        return new PrimaryExceptionRequestValidator(
            new List<IPrimaryExceptionRequestRule>
            {
                new PrimaryExceptionRequestStartsAtRangeRule(120, 255),
                new PrimaryExceptionRequestRequiredCodeRule("primary exception request needs an entry"),
                new PrimaryExceptionRequestRequiredSalaryBandRule("primary exception request needs this field"),
                new PrimaryExceptionRequestPinnedGuardRule(80),
                new PrimaryExceptionRequestAllowedTitleRule(new List<string> { "regional" }),
                new PrimaryExceptionRequestSalaryBandLengthRule(255),
            });
    }
}
