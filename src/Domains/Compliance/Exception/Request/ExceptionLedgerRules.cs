namespace Acme.Workforce.Compliance.Exception.Request;

/// <summary>A rule a exception ledger must pass. Check returns its problems, or none.</summary>
public interface IExceptionLedgerRule
{
    List<string> Check(ExceptionLedger item);
}

public sealed record ExceptionLedgerRequiredLabelRule(string Message) : IExceptionLedgerRule
{
    public List<string> Check(ExceptionLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionLedgerLabelLengthRule(int MaxLength) : IExceptionLedgerRule
{
    public List<string> Check(ExceptionLedger item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionLedgerLimitRangeRule(int Min, int Max) : IExceptionLedgerRule
{
    public List<string> Check(ExceptionLedger item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionLedgerRequiredCodeRule(string Message) : IExceptionLedgerRule
{
    public List<string> Check(ExceptionLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionLedgerBadgeLengthRule(int MaxLength) : IExceptionLedgerRule
{
    public List<string> Check(ExceptionLedger item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionLedgerRequiredBadgeRule(string Message) : IExceptionLedgerRule
{
    public List<string> Check(ExceptionLedger item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception ledger.</summary>
public sealed record ExceptionLedgerValidator(List<IExceptionLedgerRule> Rules)
{
    public List<string> Validate(ExceptionLedger item)
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

    public void RequireValid(ExceptionLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionLedger> items)
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

/// <summary>Functions of exception ledger rules.</summary>
public static class ExceptionLedgerRules
{
    public static ExceptionLedgerValidator DefaultExceptionLedgerValidator()
    {
        return new ExceptionLedgerValidator(
            new List<IExceptionLedgerRule>
            {
                new ExceptionLedgerRequiredLabelRule("exception ledger needs this field"),
                new ExceptionLedgerLabelLengthRule(120),
                new ExceptionLedgerLimitRangeRule(32, 32),
                new ExceptionLedgerRequiredCodeRule("exception ledger needs a value"),
                new ExceptionLedgerBadgeLengthRule(86400),
                new ExceptionLedgerRequiredBadgeRule("exception ledger needs an entry"),
            });
    }
}
