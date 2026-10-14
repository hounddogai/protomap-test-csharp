namespace Acme.Workforce.Workplace.Visitor.Proposal;

/// <summary>A rule a visitor exception must pass. Check returns its problems, or none.</summary>
public interface IVisitorExceptionRule
{
    List<string> Check(VisitorException item);
}

public sealed record VisitorExceptionJobCodeLengthRule(int MaxLength) : IVisitorExceptionRule
{
    public List<string> Check(VisitorException item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorExceptionStartsAtRangeRule(int Min, int Max) : IVisitorExceptionRule
{
    public List<string> Check(VisitorException item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorExceptionRequiredJobCodeRule(string Message) : IVisitorExceptionRule
{
    public List<string> Check(VisitorException item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorExceptionCodePrefixRule(string Prefix) : IVisitorExceptionRule
{
    public List<string> Check(VisitorException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorExceptionCodeLengthRule(int MaxLength) : IVisitorExceptionRule
{
    public List<string> Check(VisitorException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorExceptionRequiredTitleRule(string Message) : IVisitorExceptionRule
{
    public List<string> Check(VisitorException item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor exception.</summary>
public sealed record VisitorExceptionValidator(List<IVisitorExceptionRule> Rules)
{
    public List<string> Validate(VisitorException item)
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

    public void RequireValid(VisitorException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorException> items)
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

/// <summary>Functions of visitor exception rules.</summary>
public static class VisitorExceptionRules
{
    public static VisitorExceptionValidator DefaultVisitorExceptionValidator()
    {
        return new VisitorExceptionValidator(
            new List<IVisitorExceptionRule>
            {
                new VisitorExceptionJobCodeLengthRule(86400),
                new VisitorExceptionStartsAtRangeRule(64, 80),
                new VisitorExceptionRequiredJobCodeRule("visitor exception needs this field"),
                new VisitorExceptionCodePrefixRule("Q"),
                new VisitorExceptionCodeLengthRule(64),
                new VisitorExceptionRequiredTitleRule("visitor exception needs a value"),
            });
    }
}
