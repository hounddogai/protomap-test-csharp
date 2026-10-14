namespace Acme.Workforce.Compensation.PayBand.Template;

/// <summary>A rule a scheduled pay band baseline must pass. Check returns its problems, or none.</summary>
public interface IScheduledPayBandBaselineRule
{
    List<string> Check(ScheduledPayBandBaseline item);
}

public sealed record ScheduledPayBandBaselineScoreRangeRule(double Min, double Max) : IScheduledPayBandBaselineRule
{
    public List<string> Check(ScheduledPayBandBaseline item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledPayBandBaselineExternalRefLengthRule(int MaxLength) : IScheduledPayBandBaselineRule
{
    public List<string> Check(ScheduledPayBandBaseline item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandBaselineRequiredTitleRule(string Message) : IScheduledPayBandBaselineRule
{
    public List<string> Check(ScheduledPayBandBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandBaselineRequiredCodeRule(string Message) : IScheduledPayBandBaselineRule
{
    public List<string> Check(ScheduledPayBandBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandBaselineRequiredExternalRefRule(string Message) : IScheduledPayBandBaselineRule
{
    public List<string> Check(ScheduledPayBandBaseline item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pay band baseline.</summary>
public sealed record ScheduledPayBandBaselineValidator(List<IScheduledPayBandBaselineRule> Rules)
{
    public List<string> Validate(ScheduledPayBandBaseline item)
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

    public void RequireValid(ScheduledPayBandBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPayBandBaseline> items)
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

/// <summary>Functions of scheduled pay band baseline rules.</summary>
public static class ScheduledPayBandBaselineRules
{
    public static ScheduledPayBandBaselineValidator DefaultScheduledPayBandBaselineValidator()
    {
        return new ScheduledPayBandBaselineValidator(
            new List<IScheduledPayBandBaselineRule>
            {
                new ScheduledPayBandBaselineScoreRangeRule(3.3, 1297.6),
                new ScheduledPayBandBaselineExternalRefLengthRule(3600),
                new ScheduledPayBandBaselineRequiredTitleRule("scheduled pay band baseline needs a value"),
                new ScheduledPayBandBaselineRequiredCodeRule("scheduled pay band baseline needs this field"),
                new ScheduledPayBandBaselineRequiredExternalRefRule("scheduled pay band baseline needs this field"),
            });
    }
}
