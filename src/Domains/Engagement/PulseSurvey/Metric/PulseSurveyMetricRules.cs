namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey metric must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyMetricRule
{
    List<string> Check(PulseSurveyMetric item);
}

public sealed record PulseSurveyMetricAllowedCodeRule(List<string> Allowed) : IPulseSurveyMetricRule
{
    public List<string> Check(PulseSurveyMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyMetricVersionRangeRule(int Min, int Max) : IPulseSurveyMetricRule
{
    public List<string> Check(PulseSurveyMetric item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyMetricCodeLengthRule(int MaxLength) : IPulseSurveyMetricRule
{
    public List<string> Check(PulseSurveyMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyMetricDiscountRangeRule(double Min, double Max) : IPulseSurveyMetricRule
{
    public List<string> Check(PulseSurveyMetric item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyMetricRequiredDepartmentRule(string Message) : IPulseSurveyMetricRule
{
    public List<string> Check(PulseSurveyMetric item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey metric.</summary>
public sealed record PulseSurveyMetricValidator(List<IPulseSurveyMetricRule> Rules)
{
    public List<string> Validate(PulseSurveyMetric item)
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

    public void RequireValid(PulseSurveyMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyMetric> items)
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

/// <summary>Functions of pulse survey metric rules.</summary>
public static class PulseSurveyMetricRules
{
    public static PulseSurveyMetricValidator DefaultPulseSurveyMetricValidator()
    {
        return new PulseSurveyMetricValidator(
            new List<IPulseSurveyMetricRule>
            {
                new PulseSurveyMetricAllowedCodeRule(new List<string>()),
                new PulseSurveyMetricVersionRangeRule(80, 86400),
                new PulseSurveyMetricCodeLengthRule(255),
                new PulseSurveyMetricDiscountRangeRule(7.5, 1065.7),
                new PulseSurveyMetricRequiredDepartmentRule("pulse survey metric needs a value"),
            });
    }
}
