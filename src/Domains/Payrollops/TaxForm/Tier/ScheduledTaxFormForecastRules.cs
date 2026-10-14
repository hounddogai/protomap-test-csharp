namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled tax form forecast must pass. Check returns its problems, or none.</summary>
public interface IScheduledTaxFormForecastRule
{
    List<string> Check(ScheduledTaxFormForecast item);
}

public sealed record ScheduledTaxFormForecastFteRangeRule(double Min, double Max) : IScheduledTaxFormForecastRule
{
    public List<string> Check(ScheduledTaxFormForecast item)
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

public sealed record ScheduledTaxFormForecastRequiredTitleRule(string Message) : IScheduledTaxFormForecastRule
{
    public List<string> Check(ScheduledTaxFormForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTaxFormForecastAllowedCodeRule(List<string> Allowed) : IScheduledTaxFormForecastRule
{
    public List<string> Check(ScheduledTaxFormForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTaxFormForecastMaximumRangeRule(double Min, double Max) : IScheduledTaxFormForecastRule
{
    public List<string> Check(ScheduledTaxFormForecast item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTaxFormForecastCodeLengthRule(int MaxLength) : IScheduledTaxFormForecastRule
{
    public List<string> Check(ScheduledTaxFormForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled tax form forecast.</summary>
public sealed record ScheduledTaxFormForecastValidator(List<IScheduledTaxFormForecastRule> Rules)
{
    public List<string> Validate(ScheduledTaxFormForecast item)
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

    public void RequireValid(ScheduledTaxFormForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledTaxFormForecast> items)
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

/// <summary>Functions of scheduled tax form forecast rules.</summary>
public static class ScheduledTaxFormForecastRules
{
    public static ScheduledTaxFormForecastValidator DefaultScheduledTaxFormForecastValidator()
    {
        return new ScheduledTaxFormForecastValidator(
            new List<IScheduledTaxFormForecastRule>
            {
                new ScheduledTaxFormForecastFteRangeRule(2.2, 2368.6),
                new ScheduledTaxFormForecastRequiredTitleRule("scheduled tax form forecast needs this field"),
                new ScheduledTaxFormForecastAllowedCodeRule(new List<string>()),
                new ScheduledTaxFormForecastMaximumRangeRule(6.5, 1752.8),
                new ScheduledTaxFormForecastCodeLengthRule(86400),
            });
    }
}
