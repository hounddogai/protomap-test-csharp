namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback succession plan entry must pass. Check returns its problems, or none.</summary>
public interface IFallbackSuccessionPlanEntryRule
{
    List<string> Check(FallbackSuccessionPlanEntry item);
}

public sealed record FallbackSuccessionPlanEntryAllowedLocaleRule(List<string> Allowed) : IFallbackSuccessionPlanEntryRule
{
    public List<string> Check(FallbackSuccessionPlanEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Locale)))
        {
            return new List<string> { $"locale {item.Locale} is too long" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSuccessionPlanEntryLabelLengthRule(int MaxLength) : IFallbackSuccessionPlanEntryRule
{
    public List<string> Check(FallbackSuccessionPlanEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSuccessionPlanEntryFloorValueRangeRule(double Min, double Max) : IFallbackSuccessionPlanEntryRule
{
    public List<string> Check(FallbackSuccessionPlanEntry item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackSuccessionPlanEntryDurationMinutesRangeRule(int Min, int Max) : IFallbackSuccessionPlanEntryRule
{
    public List<string> Check(FallbackSuccessionPlanEntry item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackSuccessionPlanEntryLocaleLengthRule(int MaxLength) : IFallbackSuccessionPlanEntryRule
{
    public List<string> Check(FallbackSuccessionPlanEntry item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSuccessionPlanEntryNotesLengthRule(int MaxLength) : IFallbackSuccessionPlanEntryRule
{
    public List<string> Check(FallbackSuccessionPlanEntry item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback succession plan entry.</summary>
public sealed record FallbackSuccessionPlanEntryValidator(List<IFallbackSuccessionPlanEntryRule> Rules)
{
    public List<string> Validate(FallbackSuccessionPlanEntry item)
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

    public void RequireValid(FallbackSuccessionPlanEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackSuccessionPlanEntry> items)
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

/// <summary>Functions of fallback succession plan entry rules.</summary>
public static class FallbackSuccessionPlanEntryRules
{
    public static FallbackSuccessionPlanEntryValidator DefaultFallbackSuccessionPlanEntryValidator()
    {
        return new FallbackSuccessionPlanEntryValidator(
            new List<IFallbackSuccessionPlanEntryRule>
            {
                new FallbackSuccessionPlanEntryAllowedLocaleRule(new List<string> { "global", "seasonal", "tiered" }),
                new FallbackSuccessionPlanEntryLabelLengthRule(3600),
                new FallbackSuccessionPlanEntryFloorValueRangeRule(5.8, 3797.7),
                new FallbackSuccessionPlanEntryDurationMinutesRangeRule(32, 86400),
                new FallbackSuccessionPlanEntryLocaleLengthRule(16),
                new FallbackSuccessionPlanEntryNotesLengthRule(3600),
            });
    }
}
