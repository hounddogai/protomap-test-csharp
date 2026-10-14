namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>A rule a exception milestone must pass. Check returns its problems, or none.</summary>
public interface IExceptionMilestoneRule
{
    List<string> Check(ExceptionMilestone item);
}

public sealed record ExceptionMilestoneVersionRangeRule(int Min, int Max) : IExceptionMilestoneRule
{
    public List<string> Check(ExceptionMilestone item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionMilestoneRequiredCodeRule(string Message) : IExceptionMilestoneRule
{
    public List<string> Check(ExceptionMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionMilestoneCodePrefixRule(string Prefix) : IExceptionMilestoneRule
{
    public List<string> Check(ExceptionMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception milestone.</summary>
public sealed record ExceptionMilestoneValidator(List<IExceptionMilestoneRule> Rules)
{
    public List<string> Validate(ExceptionMilestone item)
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

    public void RequireValid(ExceptionMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionMilestone> items)
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

/// <summary>Functions of exception milestone rules.</summary>
public static class ExceptionMilestoneRules
{
    public static ExceptionMilestoneValidator DefaultExceptionMilestoneValidator()
    {
        return new ExceptionMilestoneValidator(
            new List<IExceptionMilestoneRule>
            {
                new ExceptionMilestoneVersionRangeRule(32, 64),
                new ExceptionMilestoneRequiredCodeRule("exception milestone needs a value"),
                new ExceptionMilestoneCodePrefixRule("OPS"),
            });
    }
}
