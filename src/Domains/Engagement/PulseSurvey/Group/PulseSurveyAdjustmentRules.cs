namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey adjustment must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyAdjustmentRule
{
    List<string> Check(PulseSurveyAdjustment item);
}

public sealed record PulseSurveyAdjustmentAllowedNameRule(List<string> Allowed) : IPulseSurveyAdjustmentRule
{
    public List<string> Check(PulseSurveyAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is expired" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAdjustmentCodePrefixRule(string Prefix) : IPulseSurveyAdjustmentRule
{
    public List<string> Check(PulseSurveyAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAdjustmentAttemptsRangeRule(int Min, int Max) : IPulseSurveyAdjustmentRule
{
    public List<string> Check(PulseSurveyAdjustment item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyAdjustmentRequiredCodeRule(string Message) : IPulseSurveyAdjustmentRule
{
    public List<string> Check(PulseSurveyAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey adjustment.</summary>
public sealed record PulseSurveyAdjustmentValidator(List<IPulseSurveyAdjustmentRule> Rules)
{
    public List<string> Validate(PulseSurveyAdjustment item)
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

    public void RequireValid(PulseSurveyAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyAdjustment> items)
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

/// <summary>Functions of pulse survey adjustment rules.</summary>
public static class PulseSurveyAdjustmentRules
{
    public static PulseSurveyAdjustmentValidator DefaultPulseSurveyAdjustmentValidator()
    {
        return new PulseSurveyAdjustmentValidator(
            new List<IPulseSurveyAdjustmentRule>
            {
                new PulseSurveyAdjustmentAllowedNameRule(new List<string> { "batched", "adaptive" }),
                new PulseSurveyAdjustmentCodePrefixRule("X"),
                new PulseSurveyAdjustmentAttemptsRangeRule(64, 86400),
                new PulseSurveyAdjustmentRequiredCodeRule("pulse survey adjustment needs an entry"),
            });
    }
}
