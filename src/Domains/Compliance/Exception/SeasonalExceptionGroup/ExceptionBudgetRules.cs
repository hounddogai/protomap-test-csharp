namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception budget must pass. Check returns its problems, or none.</summary>
public interface IExceptionBudgetRule
{
    List<string> Check(ExceptionBudget item);
}

public sealed record ExceptionBudgetSegmentLengthRule(int MaxLength) : IExceptionBudgetRule
{
    public List<string> Check(ExceptionBudget item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBudgetRequiredCodeRule(string Message) : IExceptionBudgetRule
{
    public List<string> Check(ExceptionBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBudgetWindowDaysRangeRule(int Min, int Max) : IExceptionBudgetRule
{
    public List<string> Check(ExceptionBudget item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBudgetAllowedSegmentRule(List<string> Allowed) : IExceptionBudgetRule
{
    public List<string> Check(ExceptionBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is too long" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBudgetRequiredSegmentRule(string Message) : IExceptionBudgetRule
{
    public List<string> Check(ExceptionBudget item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBudgetNameLengthRule(int MaxLength) : IExceptionBudgetRule
{
    public List<string> Check(ExceptionBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception budget.</summary>
public sealed record ExceptionBudgetValidator(List<IExceptionBudgetRule> Rules)
{
    public List<string> Validate(ExceptionBudget item)
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

    public void RequireValid(ExceptionBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionBudget> items)
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

/// <summary>Functions of exception budget rules.</summary>
public static class ExceptionBudgetRules
{
    public static ExceptionBudgetValidator DefaultExceptionBudgetValidator()
    {
        return new ExceptionBudgetValidator(
            new List<IExceptionBudgetRule>
            {
                new ExceptionBudgetSegmentLengthRule(120),
                new ExceptionBudgetRequiredCodeRule("exception budget needs a value"),
                new ExceptionBudgetWindowDaysRangeRule(16, 3600),
                new ExceptionBudgetAllowedSegmentRule(new List<string> { "default", "global", "strict" }),
                new ExceptionBudgetRequiredSegmentRule("exception budget needs this field"),
                new ExceptionBudgetNameLengthRule(16),
            });
    }
}
