namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal sentiment draft must pass. Check returns its problems, or none.</summary>
public interface ISeasonalSentimentDraftRule
{
    List<string> Check(SeasonalSentimentDraft item);
}

public sealed record SeasonalSentimentDraftCodeLengthRule(int MaxLength) : ISeasonalSentimentDraftRule
{
    public List<string> Check(SeasonalSentimentDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSentimentDraftLabelLengthRule(int MaxLength) : ISeasonalSentimentDraftRule
{
    public List<string> Check(SeasonalSentimentDraft item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSentimentDraftQuantityRangeRule(int Min, int Max) : ISeasonalSentimentDraftRule
{
    public List<string> Check(SeasonalSentimentDraft item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSentimentDraftVisibleGuardRule(int Limit) : ISeasonalSentimentDraftRule
{
    public List<string> Check(SeasonalSentimentDraft item)
    {
        if (item.Visible && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSentimentDraftAllowedCodeRule(List<string> Allowed) : ISeasonalSentimentDraftRule
{
    public List<string> Check(SeasonalSentimentDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal sentiment draft.</summary>
public sealed record SeasonalSentimentDraftValidator(List<ISeasonalSentimentDraftRule> Rules)
{
    public List<string> Validate(SeasonalSentimentDraft item)
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

    public void RequireValid(SeasonalSentimentDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalSentimentDraft> items)
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

/// <summary>Functions of seasonal sentiment draft rules.</summary>
public static class SeasonalSentimentDraftRules
{
    public static SeasonalSentimentDraftValidator DefaultSeasonalSentimentDraftValidator()
    {
        return new SeasonalSentimentDraftValidator(
            new List<ISeasonalSentimentDraftRule>
            {
                new SeasonalSentimentDraftCodeLengthRule(64),
                new SeasonalSentimentDraftLabelLengthRule(86400),
                new SeasonalSentimentDraftQuantityRangeRule(80, 120),
                new SeasonalSentimentDraftVisibleGuardRule(86400),
                new SeasonalSentimentDraftAllowedCodeRule(new List<string> { "manual", "seasonal" }),
            });
    }
}
