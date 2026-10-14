namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>A rule a manual pulse survey summary must pass. Check returns its problems, or none.</summary>
public interface IManualPulseSurveySummaryRule
{
    List<string> Check(ManualPulseSurveySummary item);
}

public sealed record ManualPulseSurveySummaryCodePrefixRule(string Prefix) : IManualPulseSurveySummaryRule
{
    public List<string> Check(ManualPulseSurveySummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPulseSurveySummaryMaximumRangeRule(double Min, double Max) : IManualPulseSurveySummaryRule
{
    public List<string> Check(ManualPulseSurveySummary item)
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

public sealed record ManualPulseSurveySummaryCodeLengthRule(int MaxLength) : IManualPulseSurveySummaryRule
{
    public List<string> Check(ManualPulseSurveySummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPulseSurveySummaryLabelLengthRule(int MaxLength) : IManualPulseSurveySummaryRule
{
    public List<string> Check(ManualPulseSurveySummary item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual pulse survey summary.</summary>
public sealed record ManualPulseSurveySummaryValidator(List<IManualPulseSurveySummaryRule> Rules)
{
    public List<string> Validate(ManualPulseSurveySummary item)
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

    public void RequireValid(ManualPulseSurveySummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualPulseSurveySummary> items)
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

/// <summary>Functions of manual pulse survey summary rules.</summary>
public static class ManualPulseSurveySummaryRules
{
    public static ManualPulseSurveySummaryValidator DefaultManualPulseSurveySummaryValidator()
    {
        return new ManualPulseSurveySummaryValidator(
            new List<IManualPulseSurveySummaryRule>
            {
                new ManualPulseSurveySummaryCodePrefixRule("X"),
                new ManualPulseSurveySummaryMaximumRangeRule(6.6, 1833.6),
                new ManualPulseSurveySummaryCodeLengthRule(86400),
                new ManualPulseSurveySummaryLabelLengthRule(64),
            });
    }
}
