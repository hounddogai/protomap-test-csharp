namespace Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>A rule a pulse survey rollup must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyRollupRule
{
    List<string> Check(PulseSurveyRollup item);
}

public sealed record PulseSurveyRollupCodeLengthRule(int MaxLength) : IPulseSurveyRollupRule
{
    public List<string> Check(PulseSurveyRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyRollupCodePrefixRule(string Prefix) : IPulseSurveyRollupRule
{
    public List<string> Check(PulseSurveyRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyRollupNotesLengthRule(int MaxLength) : IPulseSurveyRollupRule
{
    public List<string> Check(PulseSurveyRollup item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyRollupDiscountRangeRule(double Min, double Max) : IPulseSurveyRollupRule
{
    public List<string> Check(PulseSurveyRollup item)
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

public sealed record PulseSurveyRollupRequiredCodeRule(string Message) : IPulseSurveyRollupRule
{
    public List<string> Check(PulseSurveyRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyRollupGraceDaysRangeRule(int Min, int Max) : IPulseSurveyRollupRule
{
    public List<string> Check(PulseSurveyRollup item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pulse survey rollup.</summary>
public sealed record PulseSurveyRollupValidator(List<IPulseSurveyRollupRule> Rules)
{
    public List<string> Validate(PulseSurveyRollup item)
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

    public void RequireValid(PulseSurveyRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyRollup> items)
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

/// <summary>Functions of pulse survey rollup rules.</summary>
public static class PulseSurveyRollupRules
{
    public static PulseSurveyRollupValidator DefaultPulseSurveyRollupValidator()
    {
        return new PulseSurveyRollupValidator(
            new List<IPulseSurveyRollupRule>
            {
                new PulseSurveyRollupCodeLengthRule(16),
                new PulseSurveyRollupCodePrefixRule("X"),
                new PulseSurveyRollupNotesLengthRule(3600),
                new PulseSurveyRollupDiscountRangeRule(2.3, 580.4),
                new PulseSurveyRollupRequiredCodeRule("pulse survey rollup needs this field"),
                new PulseSurveyRollupGraceDaysRangeRule(80, 255),
            });
    }
}
