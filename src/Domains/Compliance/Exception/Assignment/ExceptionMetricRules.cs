namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception metric must pass. Check returns its problems, or none.</summary>
public interface IExceptionMetricRule
{
    List<string> Check(ExceptionMetric item);
}

public sealed record ExceptionMetricRevisionRangeRule(int Min, int Max) : IExceptionMetricRule
{
    public List<string> Check(ExceptionMetric item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionMetricCodeLengthRule(int MaxLength) : IExceptionMetricRule
{
    public List<string> Check(ExceptionMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionMetricAllowedLabelRule(List<string> Allowed) : IExceptionMetricRule
{
    public List<string> Check(ExceptionMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception metric.</summary>
public sealed record ExceptionMetricValidator(List<IExceptionMetricRule> Rules)
{
    public List<string> Validate(ExceptionMetric item)
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

    public void RequireValid(ExceptionMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionMetric> items)
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

/// <summary>Functions of exception metric rules.</summary>
public static class ExceptionMetricRules
{
    public static ExceptionMetricValidator DefaultExceptionMetricValidator()
    {
        return new ExceptionMetricValidator(
            new List<IExceptionMetricRule>
            {
                new ExceptionMetricRevisionRangeRule(32, 32),
                new ExceptionMetricCodeLengthRule(3600),
                new ExceptionMetricAllowedLabelRule(new List<string> { "regional" }),
            });
    }
}
