namespace Acme.Workforce.Compliance.Exception.Template;

/// <summary>A rule a exception adjustment must pass. Check returns its problems, or none.</summary>
public interface IExceptionAdjustmentRule
{
    List<string> Check(ExceptionAdjustment item);
}

public sealed record ExceptionAdjustmentMarginRangeRule(double Min, double Max) : IExceptionAdjustmentRule
{
    public List<string> Check(ExceptionAdjustment item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionAdjustmentHoursRangeRule(double Min, double Max) : IExceptionAdjustmentRule
{
    public List<string> Check(ExceptionAdjustment item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionAdjustmentLocationLengthRule(int MaxLength) : IExceptionAdjustmentRule
{
    public List<string> Check(ExceptionAdjustment item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionAdjustmentCodePrefixRule(string Prefix) : IExceptionAdjustmentRule
{
    public List<string> Check(ExceptionAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionAdjustmentRequiredLocationRule(string Message) : IExceptionAdjustmentRule
{
    public List<string> Check(ExceptionAdjustment item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception adjustment.</summary>
public sealed record ExceptionAdjustmentValidator(List<IExceptionAdjustmentRule> Rules)
{
    public List<string> Validate(ExceptionAdjustment item)
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

    public void RequireValid(ExceptionAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionAdjustment> items)
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

/// <summary>Functions of exception adjustment rules.</summary>
public static class ExceptionAdjustmentRules
{
    public static ExceptionAdjustmentValidator DefaultExceptionAdjustmentValidator()
    {
        return new ExceptionAdjustmentValidator(
            new List<IExceptionAdjustmentRule>
            {
                new ExceptionAdjustmentMarginRangeRule(8.6, 631.1),
                new ExceptionAdjustmentHoursRangeRule(5.8, 2791.3),
                new ExceptionAdjustmentLocationLengthRule(32),
                new ExceptionAdjustmentCodePrefixRule("X"),
                new ExceptionAdjustmentRequiredLocationRule("exception adjustment needs this field"),
            });
    }
}
