namespace Acme.Workforce.Orgdesign.Span.RecordItem;

/// <summary>A rule a pending span exception must pass. Check returns its problems, or none.</summary>
public interface IPendingSpanExceptionRule
{
    List<string> Check(PendingSpanException item);
}

public sealed record PendingSpanExceptionContactEmailLengthRule(int MaxLength) : IPendingSpanExceptionRule
{
    public List<string> Check(PendingSpanException item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSpanExceptionNameLengthRule(int MaxLength) : IPendingSpanExceptionRule
{
    public List<string> Check(PendingSpanException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSpanExceptionCodeLengthRule(int MaxLength) : IPendingSpanExceptionRule
{
    public List<string> Check(PendingSpanException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSpanExceptionRequiredManagerIdRule(string Message) : IPendingSpanExceptionRule
{
    public List<string> Check(PendingSpanException item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending span exception.</summary>
public sealed record PendingSpanExceptionValidator(List<IPendingSpanExceptionRule> Rules)
{
    public List<string> Validate(PendingSpanException item)
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

    public void RequireValid(PendingSpanException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingSpanException> items)
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

/// <summary>Functions of pending span exception rules.</summary>
public static class PendingSpanExceptionRules
{
    public static PendingSpanExceptionValidator DefaultPendingSpanExceptionValidator()
    {
        return new PendingSpanExceptionValidator(
            new List<IPendingSpanExceptionRule>
            {
                new PendingSpanExceptionContactEmailLengthRule(255),
                new PendingSpanExceptionNameLengthRule(120),
                new PendingSpanExceptionCodeLengthRule(16),
                new PendingSpanExceptionRequiredManagerIdRule("pending span exception needs an entry"),
            });
    }
}
