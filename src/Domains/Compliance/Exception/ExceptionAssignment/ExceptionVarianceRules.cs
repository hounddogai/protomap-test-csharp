namespace Acme.Workforce.Compliance.Exception.ExceptionAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception variance must pass. Check returns its problems, or none.</summary>
public interface IExceptionVarianceRule
{
    List<string> Check(ExceptionVariance item);
}

public sealed record ExceptionVarianceCurrencyLengthRule(int MaxLength) : IExceptionVarianceRule
{
    public List<string> Check(ExceptionVariance item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionVarianceTitleLengthRule(int MaxLength) : IExceptionVarianceRule
{
    public List<string> Check(ExceptionVariance item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionVarianceLimitRangeRule(int Min, int Max) : IExceptionVarianceRule
{
    public List<string> Check(ExceptionVariance item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionVarianceAllowedLocationRule(List<string> Allowed) : IExceptionVarianceRule
{
    public List<string> Check(ExceptionVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception variance.</summary>
public sealed record ExceptionVarianceValidator(List<IExceptionVarianceRule> Rules)
{
    public List<string> Validate(ExceptionVariance item)
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

    public void RequireValid(ExceptionVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionVariance> items)
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

/// <summary>Functions of exception variance rules.</summary>
public static class ExceptionVarianceRules
{
    public static ExceptionVarianceValidator DefaultExceptionVarianceValidator()
    {
        return new ExceptionVarianceValidator(
            new List<IExceptionVarianceRule>
            {
                new ExceptionVarianceCurrencyLengthRule(3600),
                new ExceptionVarianceTitleLengthRule(64),
                new ExceptionVarianceLimitRangeRule(32, 86400),
                new ExceptionVarianceAllowedLocationRule(new List<string> { "global" }),
            });
    }
}
