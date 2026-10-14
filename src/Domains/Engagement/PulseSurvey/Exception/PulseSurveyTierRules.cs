namespace Acme.Workforce.Engagement.PulseSurvey.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey tier must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyTierRule
{
    List<string> Check(PulseSurveyTier item);
}

public sealed record PulseSurveyTierRequiredNotesRule(string Message) : IPulseSurveyTierRule
{
    public List<string> Check(PulseSurveyTier item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyTierRateRangeRule(double Min, double Max) : IPulseSurveyTierRule
{
    public List<string> Check(PulseSurveyTier item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyTierCodeLengthRule(int MaxLength) : IPulseSurveyTierRule
{
    public List<string> Check(PulseSurveyTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyTierAllowedNotesRule(List<string> Allowed) : IPulseSurveyTierRule
{
    public List<string> Check(PulseSurveyTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyTierNameLengthRule(int MaxLength) : IPulseSurveyTierRule
{
    public List<string> Check(PulseSurveyTier item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyTierCodePrefixRule(string Prefix) : IPulseSurveyTierRule
{
    public List<string> Check(PulseSurveyTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey tier.</summary>
public sealed record PulseSurveyTierValidator(List<IPulseSurveyTierRule> Rules)
{
    public List<string> Validate(PulseSurveyTier item)
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

    public void RequireValid(PulseSurveyTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyTier> items)
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

/// <summary>Functions of pulse survey tier rules.</summary>
public static class PulseSurveyTierRules
{
    public static PulseSurveyTierValidator DefaultPulseSurveyTierValidator()
    {
        return new PulseSurveyTierValidator(
            new List<IPulseSurveyTierRule>
            {
                new PulseSurveyTierRequiredNotesRule("pulse survey tier needs an entry"),
                new PulseSurveyTierRateRangeRule(1.1, 2777.4),
                new PulseSurveyTierCodeLengthRule(120),
                new PulseSurveyTierAllowedNotesRule(new List<string> { "capped", "capped" }),
                new PulseSurveyTierNameLengthRule(255),
                new PulseSurveyTierCodePrefixRule("X"),
            });
    }
}
