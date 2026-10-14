namespace Acme.Workforce.Payrollops.RetroPay.Budget;

/// <summary>A rule a retro pay metric must pass. Check returns its problems, or none.</summary>
public interface IRetroPayMetricRule
{
    List<string> Check(RetroPayMetric item);
}

public sealed record RetroPayMetricDescriptionLengthRule(int MaxLength) : IRetroPayMetricRule
{
    public List<string> Check(RetroPayMetric item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayMetricRatioRangeRule(double Min, double Max) : IRetroPayMetricRule
{
    public List<string> Check(RetroPayMetric item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayMetricRequiredLocationRule(string Message) : IRetroPayMetricRule
{
    public List<string> Check(RetroPayMetric item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayMetricRequiredTitleRule(string Message) : IRetroPayMetricRule
{
    public List<string> Check(RetroPayMetric item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayMetricFteRangeRule(double Min, double Max) : IRetroPayMetricRule
{
    public List<string> Check(RetroPayMetric item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RetroPayMetricMaximumRangeRule(double Min, double Max) : IRetroPayMetricRule
{
    public List<string> Check(RetroPayMetric item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a retro pay metric.</summary>
public sealed record RetroPayMetricValidator(List<IRetroPayMetricRule> Rules)
{
    public List<string> Validate(RetroPayMetric item)
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

    public void RequireValid(RetroPayMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayMetric> items)
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

/// <summary>Functions of retro pay metric rules.</summary>
public static class RetroPayMetricRules
{
    public static RetroPayMetricValidator DefaultRetroPayMetricValidator()
    {
        return new RetroPayMetricValidator(
            new List<IRetroPayMetricRule>
            {
                new RetroPayMetricDescriptionLengthRule(64),
                new RetroPayMetricRatioRangeRule(4.1, 1932.6),
                new RetroPayMetricRequiredLocationRule("retro pay metric needs this field"),
                new RetroPayMetricRequiredTitleRule("retro pay metric needs an entry"),
                new RetroPayMetricFteRangeRule(2.9, 1987.0),
                new RetroPayMetricMaximumRangeRule(6.8, 220.7),
            });
    }
}
