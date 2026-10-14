namespace Acme.Workforce.Payrollops.RetroPay.Summary;

/// <summary>A rule a rolling retro pay summary must pass. Check returns its problems, or none.</summary>
public interface IRollingRetroPaySummaryRule
{
    List<string> Check(RollingRetroPaySummary item);
}

public sealed record RollingRetroPaySummaryRateRangeRule(double Min, double Max) : IRollingRetroPaySummaryRule
{
    public List<string> Check(RollingRetroPaySummary item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingRetroPaySummaryRatioRangeRule(double Min, double Max) : IRollingRetroPaySummaryRule
{
    public List<string> Check(RollingRetroPaySummary item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRetroPaySummaryCodePrefixRule(string Prefix) : IRollingRetroPaySummaryRule
{
    public List<string> Check(RollingRetroPaySummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRetroPaySummaryCurrencyLengthRule(int MaxLength) : IRollingRetroPaySummaryRule
{
    public List<string> Check(RollingRetroPaySummary item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRetroPaySummaryChannelLengthRule(int MaxLength) : IRollingRetroPaySummaryRule
{
    public List<string> Check(RollingRetroPaySummary item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling retro pay summary.</summary>
public sealed record RollingRetroPaySummaryValidator(List<IRollingRetroPaySummaryRule> Rules)
{
    public List<string> Validate(RollingRetroPaySummary item)
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

    public void RequireValid(RollingRetroPaySummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRetroPaySummary> items)
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

/// <summary>Functions of rolling retro pay summary rules.</summary>
public static class RollingRetroPaySummaryRules
{
    public static RollingRetroPaySummaryValidator DefaultRollingRetroPaySummaryValidator()
    {
        return new RollingRetroPaySummaryValidator(
            new List<IRollingRetroPaySummaryRule>
            {
                new RollingRetroPaySummaryRateRangeRule(4.6, 1387.0),
                new RollingRetroPaySummaryRatioRangeRule(7.8, 1366.3),
                new RollingRetroPaySummaryCodePrefixRule("Q"),
                new RollingRetroPaySummaryCurrencyLengthRule(1000),
                new RollingRetroPaySummaryChannelLengthRule(1000),
            });
    }
}
