namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span forecast must pass. Check returns its problems, or none.</summary>
public interface ISpanForecastRule
{
    List<string> Check(SpanForecast item);
}

public sealed record SpanForecastAllowedLocationRule(List<string> Allowed) : ISpanForecastRule
{
    public List<string> Check(SpanForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record SpanForecastMaximumRangeRule(double Min, double Max) : ISpanForecastRule
{
    public List<string> Check(SpanForecast item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanForecastRequiredCategoryRule(string Message) : ISpanForecastRule
{
    public List<string> Check(SpanForecast item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanForecastRequiredLabelRule(string Message) : ISpanForecastRule
{
    public List<string> Check(SpanForecast item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanForecastEndsAtRangeRule(int Min, int Max) : ISpanForecastRule
{
    public List<string> Check(SpanForecast item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanForecastExpiresAtRangeRule(int Min, int Max) : ISpanForecastRule
{
    public List<string> Check(SpanForecast item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span forecast.</summary>
public sealed record SpanForecastValidator(List<ISpanForecastRule> Rules)
{
    public List<string> Validate(SpanForecast item)
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

    public void RequireValid(SpanForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanForecast> items)
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

/// <summary>Functions of span forecast rules.</summary>
public static class SpanForecastRules
{
    public static SpanForecastValidator DefaultSpanForecastValidator()
    {
        return new SpanForecastValidator(
            new List<ISpanForecastRule>
            {
                new SpanForecastAllowedLocationRule(new List<string> { "weighted" }),
                new SpanForecastMaximumRangeRule(5.0, 2780.3),
                new SpanForecastRequiredCategoryRule("span forecast needs this field"),
                new SpanForecastRequiredLabelRule("span forecast needs this field"),
                new SpanForecastEndsAtRangeRule(32, 86400),
                new SpanForecastExpiresAtRangeRule(16, 86400),
            });
    }
}
