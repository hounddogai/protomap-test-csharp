namespace Acme.Workforce.Workplace.Visitor.Exception;

/// <summary>A rule a lenient visitor exception must pass. Check returns its problems, or none.</summary>
public interface ILenientVisitorExceptionRule
{
    List<string> Check(LenientVisitorException item);
}

public sealed record LenientVisitorExceptionNameLengthRule(int MaxLength) : ILenientVisitorExceptionRule
{
    public List<string> Check(LenientVisitorException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorExceptionRetriesRangeRule(int Min, int Max) : ILenientVisitorExceptionRule
{
    public List<string> Check(LenientVisitorException item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientVisitorExceptionRequiredNameRule(string Message) : ILenientVisitorExceptionRule
{
    public List<string> Check(LenientVisitorException item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient visitor exception.</summary>
public sealed record LenientVisitorExceptionValidator(List<ILenientVisitorExceptionRule> Rules)
{
    public List<string> Validate(LenientVisitorException item)
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

    public void RequireValid(LenientVisitorException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientVisitorException> items)
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

/// <summary>Functions of lenient visitor exception rules.</summary>
public static class LenientVisitorExceptionRules
{
    public static LenientVisitorExceptionValidator DefaultLenientVisitorExceptionValidator()
    {
        return new LenientVisitorExceptionValidator(
            new List<ILenientVisitorExceptionRule>
            {
                new LenientVisitorExceptionNameLengthRule(80),
                new LenientVisitorExceptionRetriesRangeRule(64, 3600),
                new LenientVisitorExceptionRequiredNameRule("lenient visitor exception needs a value"),
            });
    }
}
