namespace Acme.Workforce.Compliance.Exception.Baseline;

/// <summary>A rule a exception baseline must pass. Check returns its problems, or none.</summary>
public interface IExceptionBaselineRule
{
    List<string> Check(ExceptionBaseline item);
}

public sealed record ExceptionBaselineExternalRefLengthRule(int MaxLength) : IExceptionBaselineRule
{
    public List<string> Check(ExceptionBaseline item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBaselineRetriesRangeRule(int Min, int Max) : IExceptionBaselineRule
{
    public List<string> Check(ExceptionBaseline item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBaselineTenureMonthsRangeRule(int Min, int Max) : IExceptionBaselineRule
{
    public List<string> Check(ExceptionBaseline item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception baseline.</summary>
public sealed record ExceptionBaselineValidator(List<IExceptionBaselineRule> Rules)
{
    public List<string> Validate(ExceptionBaseline item)
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

    public void RequireValid(ExceptionBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionBaseline> items)
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

/// <summary>Functions of exception baseline rules.</summary>
public static class ExceptionBaselineRules
{
    public static ExceptionBaselineValidator DefaultExceptionBaselineValidator()
    {
        return new ExceptionBaselineValidator(
            new List<IExceptionBaselineRule>
            {
                new ExceptionBaselineExternalRefLengthRule(16),
                new ExceptionBaselineRetriesRangeRule(255, 1000),
                new ExceptionBaselineTenureMonthsRangeRule(80, 255),
            });
    }
}
