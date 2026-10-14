namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient leave request forecast must pass. Check returns its problems, or none.</summary>
public interface ILenientLeaveRequestForecastRule
{
    List<string> Check(LenientLeaveRequestForecast item);
}

public sealed record LenientLeaveRequestForecastCodePrefixRule(string Prefix) : ILenientLeaveRequestForecastRule
{
    public List<string> Check(LenientLeaveRequestForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLeaveRequestForecastAllowedCodeRule(List<string> Allowed) : ILenientLeaveRequestForecastRule
{
    public List<string> Check(LenientLeaveRequestForecast item)
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

public sealed record LenientLeaveRequestForecastTagsLimitRule(int MaxCount, List<string> Banned) : ILenientLeaveRequestForecastRule
{
    public List<string> Check(LenientLeaveRequestForecast item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record LenientLeaveRequestForecastRequiredLabelRule(string Message) : ILenientLeaveRequestForecastRule
{
    public List<string> Check(LenientLeaveRequestForecast item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientLeaveRequestForecastMinimumRangeRule(double Min, double Max) : ILenientLeaveRequestForecastRule
{
    public List<string> Check(LenientLeaveRequestForecast item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLeaveRequestForecastLocationLengthRule(int MaxLength) : ILenientLeaveRequestForecastRule
{
    public List<string> Check(LenientLeaveRequestForecast item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient leave request forecast.</summary>
public sealed record LenientLeaveRequestForecastValidator(List<ILenientLeaveRequestForecastRule> Rules)
{
    public List<string> Validate(LenientLeaveRequestForecast item)
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

    public void RequireValid(LenientLeaveRequestForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientLeaveRequestForecast> items)
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

/// <summary>Functions of lenient leave request forecast rules.</summary>
public static class LenientLeaveRequestForecastRules
{
    public static LenientLeaveRequestForecastValidator DefaultLenientLeaveRequestForecastValidator()
    {
        return new LenientLeaveRequestForecastValidator(
            new List<ILenientLeaveRequestForecastRule>
            {
                new LenientLeaveRequestForecastCodePrefixRule("REF"),
                new LenientLeaveRequestForecastAllowedCodeRule(new List<string> { "weighted" }),
                new LenientLeaveRequestForecastTagsLimitRule(1000, new List<string> { "lenient" }),
                new LenientLeaveRequestForecastRequiredLabelRule("lenient leave request forecast needs a value"),
                new LenientLeaveRequestForecastMinimumRangeRule(7.5, 4306.9),
                new LenientLeaveRequestForecastLocationLengthRule(16),
            });
    }
}
