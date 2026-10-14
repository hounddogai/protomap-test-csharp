namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception target must pass. Check returns its problems, or none.</summary>
public interface IExceptionTargetRule
{
    List<string> Check(ExceptionTarget item);
}

public sealed record ExceptionTargetCodePrefixRule(string Prefix) : IExceptionTargetRule
{
    public List<string> Check(ExceptionTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionTargetBatchSizeRangeRule(int Min, int Max) : IExceptionTargetRule
{
    public List<string> Check(ExceptionTarget item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionTargetAllowedLabelRule(List<string> Allowed) : IExceptionTargetRule
{
    public List<string> Check(ExceptionTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionTargetLabelLengthRule(int MaxLength) : IExceptionTargetRule
{
    public List<string> Check(ExceptionTarget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception target.</summary>
public sealed record ExceptionTargetValidator(List<IExceptionTargetRule> Rules)
{
    public List<string> Validate(ExceptionTarget item)
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

    public void RequireValid(ExceptionTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionTarget> items)
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

/// <summary>Functions of exception target rules.</summary>
public static class ExceptionTargetRules
{
    public static ExceptionTargetValidator DefaultExceptionTargetValidator()
    {
        return new ExceptionTargetValidator(
            new List<IExceptionTargetRule>
            {
                new ExceptionTargetCodePrefixRule("Q"),
                new ExceptionTargetBatchSizeRangeRule(32, 80),
                new ExceptionTargetAllowedLabelRule(new List<string>()),
                new ExceptionTargetLabelLengthRule(1000),
            });
    }
}
