namespace Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>A rule a town hall template must pass. Check returns its problems, or none.</summary>
public interface ITownHallTemplateRule
{
    List<string> Check(TownHallTemplate item);
}

public sealed record TownHallTemplateRequiredCodeRule(string Message) : ITownHallTemplateRule
{
    public List<string> Check(TownHallTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallTemplateRequiredNotesRule(string Message) : ITownHallTemplateRule
{
    public List<string> Check(TownHallTemplate item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallTemplateRequiredSummaryRule(string Message) : ITownHallTemplateRule
{
    public List<string> Check(TownHallTemplate item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallTemplateCodeLengthRule(int MaxLength) : ITownHallTemplateRule
{
    public List<string> Check(TownHallTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallTemplateCodePrefixRule(string Prefix) : ITownHallTemplateRule
{
    public List<string> Check(TownHallTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallTemplateEndsAtRangeRule(int Min, int Max) : ITownHallTemplateRule
{
    public List<string> Check(TownHallTemplate item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall template.</summary>
public sealed record TownHallTemplateValidator(List<ITownHallTemplateRule> Rules)
{
    public List<string> Validate(TownHallTemplate item)
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

    public void RequireValid(TownHallTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallTemplate> items)
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

/// <summary>Functions of town hall template rules.</summary>
public static class TownHallTemplateRules
{
    public static TownHallTemplateValidator DefaultTownHallTemplateValidator()
    {
        return new TownHallTemplateValidator(
            new List<ITownHallTemplateRule>
            {
                new TownHallTemplateRequiredCodeRule("town hall template needs a value"),
                new TownHallTemplateRequiredNotesRule("town hall template needs an entry"),
                new TownHallTemplateRequiredSummaryRule("town hall template needs this field"),
                new TownHallTemplateCodeLengthRule(1000),
                new TownHallTemplateCodePrefixRule("REF"),
                new TownHallTemplateEndsAtRangeRule(64, 120),
            });
    }
}
