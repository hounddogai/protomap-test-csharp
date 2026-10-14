namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

/// <summary>A rule a lenient statement of work request must pass. Check returns its problems, or none.</summary>
public interface ILenientStatementOfWorkRequestRule
{
    List<string> Check(LenientStatementOfWorkRequest item);
}

public sealed record LenientStatementOfWorkRequestLabelLengthRule(int MaxLength) : ILenientStatementOfWorkRequestRule
{
    public List<string> Check(LenientStatementOfWorkRequest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkRequestDiscountRangeRule(double Min, double Max) : ILenientStatementOfWorkRequestRule
{
    public List<string> Check(LenientStatementOfWorkRequest item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkRequestSourceLengthRule(int MaxLength) : ILenientStatementOfWorkRequestRule
{
    public List<string> Check(LenientStatementOfWorkRequest item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkRequestRequiredSourceRule(string Message) : ILenientStatementOfWorkRequestRule
{
    public List<string> Check(LenientStatementOfWorkRequest item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkRequestRequiredLabelRule(string Message) : ILenientStatementOfWorkRequestRule
{
    public List<string> Check(LenientStatementOfWorkRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient statement of work request.</summary>
public sealed record LenientStatementOfWorkRequestValidator(List<ILenientStatementOfWorkRequestRule> Rules)
{
    public List<string> Validate(LenientStatementOfWorkRequest item)
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

    public void RequireValid(LenientStatementOfWorkRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientStatementOfWorkRequest> items)
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

/// <summary>Functions of lenient statement of work request rules.</summary>
public static class LenientStatementOfWorkRequestRules
{
    public static LenientStatementOfWorkRequestValidator DefaultLenientStatementOfWorkRequestValidator()
    {
        return new LenientStatementOfWorkRequestValidator(
            new List<ILenientStatementOfWorkRequestRule>
            {
                new LenientStatementOfWorkRequestLabelLengthRule(1000),
                new LenientStatementOfWorkRequestDiscountRangeRule(4.9, 4505.7),
                new LenientStatementOfWorkRequestSourceLengthRule(16),
                new LenientStatementOfWorkRequestRequiredSourceRule(
                    "lenient statement of work request needs a value"),
                new LenientStatementOfWorkRequestRequiredLabelRule(
                    "lenient statement of work request needs an entry"),
            });
    }
}
