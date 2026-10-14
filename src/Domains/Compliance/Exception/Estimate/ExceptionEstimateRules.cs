namespace Acme.Workforce.Compliance.Exception.Estimate;

/// <summary>A rule a exception estimate must pass. Check returns its problems, or none.</summary>
public interface IExceptionEstimateRule
{
    List<string> Check(ExceptionEstimate item);
}

public sealed record ExceptionEstimateRequiredTimezoneRule(string Message) : IExceptionEstimateRule
{
    public List<string> Check(ExceptionEstimate item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionEstimateCodePrefixRule(string Prefix) : IExceptionEstimateRule
{
    public List<string> Check(ExceptionEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionEstimateRequiredJobCodeRule(string Message) : IExceptionEstimateRule
{
    public List<string> Check(ExceptionEstimate item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionEstimateScoreRangeRule(double Min, double Max) : IExceptionEstimateRule
{
    public List<string> Check(ExceptionEstimate item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionEstimateJobCodeLengthRule(int MaxLength) : IExceptionEstimateRule
{
    public List<string> Check(ExceptionEstimate item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionEstimateRatioRangeRule(double Min, double Max) : IExceptionEstimateRule
{
    public List<string> Check(ExceptionEstimate item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a exception estimate.</summary>
public sealed record ExceptionEstimateValidator(List<IExceptionEstimateRule> Rules)
{
    public List<string> Validate(ExceptionEstimate item)
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

    public void RequireValid(ExceptionEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionEstimate> items)
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

/// <summary>Functions of exception estimate rules.</summary>
public static class ExceptionEstimateRules
{
    public static ExceptionEstimateValidator DefaultExceptionEstimateValidator()
    {
        return new ExceptionEstimateValidator(
            new List<IExceptionEstimateRule>
            {
                new ExceptionEstimateRequiredTimezoneRule("exception estimate needs this field"),
                new ExceptionEstimateCodePrefixRule("Q"),
                new ExceptionEstimateRequiredJobCodeRule("exception estimate needs an entry"),
                new ExceptionEstimateScoreRangeRule(8.2, 2807.5),
                new ExceptionEstimateJobCodeLengthRule(120),
                new ExceptionEstimateRatioRangeRule(2.8, 4708.9),
            });
    }
}
