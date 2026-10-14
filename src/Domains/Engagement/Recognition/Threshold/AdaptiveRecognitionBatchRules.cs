namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive recognition batch must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveRecognitionBatchRule
{
    List<string> Check(AdaptiveRecognitionBatch item);
}

public sealed record AdaptiveRecognitionBatchAllowedDescriptionRule(List<string> Allowed) : IAdaptiveRecognitionBatchRule
{
    public List<string> Check(AdaptiveRecognitionBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is too long" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRecognitionBatchRequiredLocationRule(string Message) : IAdaptiveRecognitionBatchRule
{
    public List<string> Check(AdaptiveRecognitionBatch item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRecognitionBatchRequiredNotesRule(string Message) : IAdaptiveRecognitionBatchRule
{
    public List<string> Check(AdaptiveRecognitionBatch item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRecognitionBatchCodePrefixRule(string Prefix) : IAdaptiveRecognitionBatchRule
{
    public List<string> Check(AdaptiveRecognitionBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRecognitionBatchLocationLengthRule(int MaxLength) : IAdaptiveRecognitionBatchRule
{
    public List<string> Check(AdaptiveRecognitionBatch item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRecognitionBatchLabelLengthRule(int MaxLength) : IAdaptiveRecognitionBatchRule
{
    public List<string> Check(AdaptiveRecognitionBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive recognition batch.</summary>
public sealed record AdaptiveRecognitionBatchValidator(List<IAdaptiveRecognitionBatchRule> Rules)
{
    public List<string> Validate(AdaptiveRecognitionBatch item)
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

    public void RequireValid(AdaptiveRecognitionBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveRecognitionBatch> items)
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

/// <summary>Functions of adaptive recognition batch rules.</summary>
public static class AdaptiveRecognitionBatchRules
{
    public static AdaptiveRecognitionBatchValidator DefaultAdaptiveRecognitionBatchValidator()
    {
        return new AdaptiveRecognitionBatchValidator(
            new List<IAdaptiveRecognitionBatchRule>
            {
                new AdaptiveRecognitionBatchAllowedDescriptionRule(new List<string> { "legacy", "manual" }),
                new AdaptiveRecognitionBatchRequiredLocationRule("adaptive recognition batch needs an entry"),
                new AdaptiveRecognitionBatchRequiredNotesRule("adaptive recognition batch needs this field"),
                new AdaptiveRecognitionBatchCodePrefixRule("REF"),
                new AdaptiveRecognitionBatchLocationLengthRule(255),
                new AdaptiveRecognitionBatchLabelLengthRule(64),
            });
    }
}
