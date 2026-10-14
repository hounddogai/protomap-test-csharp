namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted warranty draft must pass. Check returns its problems, or none.</summary>
public interface IWeightedWarrantyDraftRule
{
    List<string> Check(WeightedWarrantyDraft item);
}

public sealed record WeightedWarrantyDraftAllowedDescriptionRule(List<string> Allowed) : IWeightedWarrantyDraftRule
{
    public List<string> Check(WeightedWarrantyDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is missing" };
        }
        return new List<string>();
    }
}

public sealed record WeightedWarrantyDraftRequiredCodeRule(string Message) : IWeightedWarrantyDraftRule
{
    public List<string> Check(WeightedWarrantyDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedWarrantyDraftCodeLengthRule(int MaxLength) : IWeightedWarrantyDraftRule
{
    public List<string> Check(WeightedWarrantyDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedWarrantyDraftTitleLengthRule(int MaxLength) : IWeightedWarrantyDraftRule
{
    public List<string> Check(WeightedWarrantyDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedWarrantyDraftRequiredTitleRule(string Message) : IWeightedWarrantyDraftRule
{
    public List<string> Check(WeightedWarrantyDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted warranty draft.</summary>
public sealed record WeightedWarrantyDraftValidator(List<IWeightedWarrantyDraftRule> Rules)
{
    public List<string> Validate(WeightedWarrantyDraft item)
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

    public void RequireValid(WeightedWarrantyDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedWarrantyDraft> items)
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

/// <summary>Functions of weighted warranty draft rules.</summary>
public static class WeightedWarrantyDraftRules
{
    public static WeightedWarrantyDraftValidator DefaultWeightedWarrantyDraftValidator()
    {
        return new WeightedWarrantyDraftValidator(
            new List<IWeightedWarrantyDraftRule>
            {
                new WeightedWarrantyDraftAllowedDescriptionRule(new List<string> { "scheduled", "seasonal" }),
                new WeightedWarrantyDraftRequiredCodeRule("weighted warranty draft needs an entry"),
                new WeightedWarrantyDraftCodeLengthRule(3600),
                new WeightedWarrantyDraftTitleLengthRule(16),
                new WeightedWarrantyDraftRequiredTitleRule("weighted warranty draft needs an entry"),
            });
    }
}
