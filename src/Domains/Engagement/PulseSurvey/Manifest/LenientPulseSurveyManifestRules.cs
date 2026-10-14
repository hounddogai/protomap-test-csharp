namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient pulse survey manifest must pass. Check returns its problems, or none.</summary>
public interface ILenientPulseSurveyManifestRule
{
    List<string> Check(LenientPulseSurveyManifest item);
}

public sealed record LenientPulseSurveyManifestMinimumRangeRule(double Min, double Max) : ILenientPulseSurveyManifestRule
{
    public List<string> Check(LenientPulseSurveyManifest item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientPulseSurveyManifestSequenceRangeRule(int Min, int Max) : ILenientPulseSurveyManifestRule
{
    public List<string> Check(LenientPulseSurveyManifest item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientPulseSurveyManifestVisibleGuardRule(double Limit) : ILenientPulseSurveyManifestRule
{
    public List<string> Check(LenientPulseSurveyManifest item)
    {
        if (item.Visible && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record LenientPulseSurveyManifestAllowedLabelRule(List<string> Allowed) : ILenientPulseSurveyManifestRule
{
    public List<string> Check(LenientPulseSurveyManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient pulse survey manifest.</summary>
public sealed record LenientPulseSurveyManifestValidator(List<ILenientPulseSurveyManifestRule> Rules)
{
    public List<string> Validate(LenientPulseSurveyManifest item)
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

    public void RequireValid(LenientPulseSurveyManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPulseSurveyManifest> items)
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

/// <summary>Functions of lenient pulse survey manifest rules.</summary>
public static class LenientPulseSurveyManifestRules
{
    public static LenientPulseSurveyManifestValidator DefaultLenientPulseSurveyManifestValidator()
    {
        return new LenientPulseSurveyManifestValidator(
            new List<ILenientPulseSurveyManifestRule>
            {
                new LenientPulseSurveyManifestMinimumRangeRule(3.0, 4764.2),
                new LenientPulseSurveyManifestSequenceRangeRule(120, 1000),
                new LenientPulseSurveyManifestVisibleGuardRule(9.7),
                new LenientPulseSurveyManifestAllowedLabelRule(new List<string>()),
            });
    }
}
