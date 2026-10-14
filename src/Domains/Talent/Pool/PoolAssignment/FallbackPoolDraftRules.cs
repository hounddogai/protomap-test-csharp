namespace Acme.Workforce.Talent.Pool.PoolAssignment;

/// <summary>A rule a fallback pool draft must pass. Check returns its problems, or none.</summary>
public interface IFallbackPoolDraftRule
{
    List<string> Check(FallbackPoolDraft item);
}

public sealed record FallbackPoolDraftCodeLengthRule(int MaxLength) : IFallbackPoolDraftRule
{
    public List<string> Check(FallbackPoolDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPoolDraftLocaleLengthRule(int MaxLength) : IFallbackPoolDraftRule
{
    public List<string> Check(FallbackPoolDraft item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPoolDraftCeilingRangeRule(double Min, double Max) : IFallbackPoolDraftRule
{
    public List<string> Check(FallbackPoolDraft item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPoolDraftRequiredLabelRule(string Message) : IFallbackPoolDraftRule
{
    public List<string> Check(FallbackPoolDraft item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback pool draft.</summary>
public sealed record FallbackPoolDraftValidator(List<IFallbackPoolDraftRule> Rules)
{
    public List<string> Validate(FallbackPoolDraft item)
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

    public void RequireValid(FallbackPoolDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPoolDraft> items)
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

/// <summary>Functions of fallback pool draft rules.</summary>
public static class FallbackPoolDraftRules
{
    public static FallbackPoolDraftValidator DefaultFallbackPoolDraftValidator()
    {
        return new FallbackPoolDraftValidator(
            new List<IFallbackPoolDraftRule>
            {
                new FallbackPoolDraftCodeLengthRule(32),
                new FallbackPoolDraftLocaleLengthRule(16),
                new FallbackPoolDraftCeilingRangeRule(0.9, 1026.5),
                new FallbackPoolDraftRequiredLabelRule("fallback pool draft needs this field"),
            });
    }
}
