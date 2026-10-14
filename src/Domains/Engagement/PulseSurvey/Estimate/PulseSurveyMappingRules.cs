namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey mapping must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyMappingRule
{
    List<string> Check(PulseSurveyMapping item);
}

public sealed record PulseSurveyMappingSequenceRangeRule(int Min, int Max) : IPulseSurveyMappingRule
{
    public List<string> Check(PulseSurveyMapping item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyMappingLabelLengthRule(int MaxLength) : IPulseSurveyMappingRule
{
    public List<string> Check(PulseSurveyMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyMappingCodePrefixRule(string Prefix) : IPulseSurveyMappingRule
{
    public List<string> Check(PulseSurveyMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyMappingAllowedCodeRule(List<string> Allowed) : IPulseSurveyMappingRule
{
    public List<string> Check(PulseSurveyMapping item)
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

public sealed record PulseSurveyMappingEndsAtRangeRule(int Min, int Max) : IPulseSurveyMappingRule
{
    public List<string> Check(PulseSurveyMapping item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey mapping.</summary>
public sealed record PulseSurveyMappingValidator(List<IPulseSurveyMappingRule> Rules)
{
    public List<string> Validate(PulseSurveyMapping item)
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

    public void RequireValid(PulseSurveyMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyMapping> items)
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

/// <summary>Functions of pulse survey mapping rules.</summary>
public static class PulseSurveyMappingRules
{
    public static PulseSurveyMappingValidator DefaultPulseSurveyMappingValidator()
    {
        return new PulseSurveyMappingValidator(
            new List<IPulseSurveyMappingRule>
            {
                new PulseSurveyMappingSequenceRangeRule(255, 1000),
                new PulseSurveyMappingLabelLengthRule(64),
                new PulseSurveyMappingCodePrefixRule("OPS"),
                new PulseSurveyMappingAllowedCodeRule(new List<string> { "global", "lenient" }),
                new PulseSurveyMappingEndsAtRangeRule(255, 1000),
            });
    }
}
