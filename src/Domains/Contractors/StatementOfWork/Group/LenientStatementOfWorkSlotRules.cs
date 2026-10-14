namespace Acme.Workforce.Contractors.StatementOfWork.Group;

/// <summary>A rule a lenient statement of work slot must pass. Check returns its problems, or none.</summary>
public interface ILenientStatementOfWorkSlotRule
{
    List<string> Check(LenientStatementOfWorkSlot item);
}

public sealed record LenientStatementOfWorkSlotCodePrefixRule(string Prefix) : ILenientStatementOfWorkSlotRule
{
    public List<string> Check(LenientStatementOfWorkSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkSlotCodeLengthRule(int MaxLength) : ILenientStatementOfWorkSlotRule
{
    public List<string> Check(LenientStatementOfWorkSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkSlotRequiredEmployeeIdRule(string Message) : ILenientStatementOfWorkSlotRule
{
    public List<string> Check(LenientStatementOfWorkSlot item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkSlotRequiredNameRule(string Message) : ILenientStatementOfWorkSlotRule
{
    public List<string> Check(LenientStatementOfWorkSlot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient statement of work slot.</summary>
public sealed record LenientStatementOfWorkSlotValidator(List<ILenientStatementOfWorkSlotRule> Rules)
{
    public List<string> Validate(LenientStatementOfWorkSlot item)
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

    public void RequireValid(LenientStatementOfWorkSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientStatementOfWorkSlot> items)
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

/// <summary>Functions of lenient statement of work slot rules.</summary>
public static class LenientStatementOfWorkSlotRules
{
    public static LenientStatementOfWorkSlotValidator DefaultLenientStatementOfWorkSlotValidator()
    {
        return new LenientStatementOfWorkSlotValidator(
            new List<ILenientStatementOfWorkSlotRule>
            {
                new LenientStatementOfWorkSlotCodePrefixRule("Q"),
                new LenientStatementOfWorkSlotCodeLengthRule(3600),
                new LenientStatementOfWorkSlotRequiredEmployeeIdRule("lenient statement of work slot needs an entry"),
                new LenientStatementOfWorkSlotRequiredNameRule("lenient statement of work slot needs an entry"),
            });
    }
}
