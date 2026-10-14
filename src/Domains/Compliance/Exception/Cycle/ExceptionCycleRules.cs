namespace Acme.Workforce.Compliance.Exception.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception cycle must pass. Check returns its problems, or none.</summary>
public interface IExceptionCycleRule
{
    List<string> Check(ExceptionCycle item);
}

public sealed record ExceptionCycleRequiredLabelRule(string Message) : IExceptionCycleRule
{
    public List<string> Check(ExceptionCycle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionCycleWindowDaysRangeRule(int Min, int Max) : IExceptionCycleRule
{
    public List<string> Check(ExceptionCycle item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionCycleLabelLengthRule(int MaxLength) : IExceptionCycleRule
{
    public List<string> Check(ExceptionCycle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionCycleAllowedCodeRule(List<string> Allowed) : IExceptionCycleRule
{
    public List<string> Check(ExceptionCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionCycleCodeLengthRule(int MaxLength) : IExceptionCycleRule
{
    public List<string> Check(ExceptionCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionCycleRequiredCodeRule(string Message) : IExceptionCycleRule
{
    public List<string> Check(ExceptionCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception cycle.</summary>
public sealed record ExceptionCycleValidator(List<IExceptionCycleRule> Rules)
{
    public List<string> Validate(ExceptionCycle item)
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

    public void RequireValid(ExceptionCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionCycle> items)
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

/// <summary>Functions of exception cycle rules.</summary>
public static class ExceptionCycleRules
{
    public static ExceptionCycleValidator DefaultExceptionCycleValidator()
    {
        return new ExceptionCycleValidator(
            new List<IExceptionCycleRule>
            {
                new ExceptionCycleRequiredLabelRule("exception cycle needs this field"),
                new ExceptionCycleWindowDaysRangeRule(3600, 86400),
                new ExceptionCycleLabelLengthRule(80),
                new ExceptionCycleAllowedCodeRule(new List<string>()),
                new ExceptionCycleCodeLengthRule(3600),
                new ExceptionCycleRequiredCodeRule("exception cycle needs this field"),
            });
    }
}
