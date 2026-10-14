namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive pool draft must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePoolDraftRule
{
    List<string> Check(AdaptivePoolDraft item);
}

public sealed record AdaptivePoolDraftRequiredLabelRule(string Message) : IAdaptivePoolDraftRule
{
    public List<string> Check(AdaptivePoolDraft item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePoolDraftAllowedCodeRule(List<string> Allowed) : IAdaptivePoolDraftRule
{
    public List<string> Check(AdaptivePoolDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePoolDraftGradeLengthRule(int MaxLength) : IAdaptivePoolDraftRule
{
    public List<string> Check(AdaptivePoolDraft item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePoolDraftDiscountRangeRule(double Min, double Max) : IAdaptivePoolDraftRule
{
    public List<string> Check(AdaptivePoolDraft item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive pool draft.</summary>
public sealed record AdaptivePoolDraftValidator(List<IAdaptivePoolDraftRule> Rules)
{
    public List<string> Validate(AdaptivePoolDraft item)
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

    public void RequireValid(AdaptivePoolDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePoolDraft> items)
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

/// <summary>Functions of adaptive pool draft rules.</summary>
public static class AdaptivePoolDraftRules
{
    public static AdaptivePoolDraftValidator DefaultAdaptivePoolDraftValidator()
    {
        return new AdaptivePoolDraftValidator(
            new List<IAdaptivePoolDraftRule>
            {
                new AdaptivePoolDraftRequiredLabelRule("adaptive pool draft needs a value"),
                new AdaptivePoolDraftAllowedCodeRule(new List<string> { "lenient" }),
                new AdaptivePoolDraftGradeLengthRule(120),
                new AdaptivePoolDraftDiscountRangeRule(1.8, 4865.0),
            });
    }
}
