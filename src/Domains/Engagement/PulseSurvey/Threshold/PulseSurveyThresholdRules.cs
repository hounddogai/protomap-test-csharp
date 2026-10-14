namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey threshold must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyThresholdRule
{
    List<string> Check(PulseSurveyThreshold item);
}

public sealed record PulseSurveyThresholdRequiredCodeRule(string Message) : IPulseSurveyThresholdRule
{
    public List<string> Check(PulseSurveyThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyThresholdThresholdRangeRule(double Min, double Max) : IPulseSurveyThresholdRule
{
    public List<string> Check(PulseSurveyThreshold item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyThresholdRequiredSalaryBandRule(string Message) : IPulseSurveyThresholdRule
{
    public List<string> Check(PulseSurveyThreshold item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyThresholdAllowedLabelRule(List<string> Allowed) : IPulseSurveyThresholdRule
{
    public List<string> Check(PulseSurveyThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey threshold.</summary>
public sealed record PulseSurveyThresholdValidator(List<IPulseSurveyThresholdRule> Rules)
{
    public List<string> Validate(PulseSurveyThreshold item)
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

    public void RequireValid(PulseSurveyThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyThreshold> items)
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

/// <summary>Functions of pulse survey threshold rules.</summary>
public static class PulseSurveyThresholdRules
{
    public static PulseSurveyThresholdValidator DefaultPulseSurveyThresholdValidator()
    {
        return new PulseSurveyThresholdValidator(
            new List<IPulseSurveyThresholdRule>
            {
                new PulseSurveyThresholdRequiredCodeRule("pulse survey threshold needs this field"),
                new PulseSurveyThresholdThresholdRangeRule(0.6, 2771.8),
                new PulseSurveyThresholdRequiredSalaryBandRule("pulse survey threshold needs a value"),
                new PulseSurveyThresholdAllowedLabelRule(new List<string> { "capped", "global", "adaptive" }),
            });
    }
}
