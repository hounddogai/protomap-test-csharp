namespace Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>A rule a adaptive warranty target must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveWarrantyTargetRule
{
    List<string> Check(AdaptiveWarrantyTarget item);
}

public sealed record AdaptiveWarrantyTargetDescriptionLengthRule(int MaxLength) : IAdaptiveWarrantyTargetRule
{
    public List<string> Check(AdaptiveWarrantyTarget item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyTargetCodeLengthRule(int MaxLength) : IAdaptiveWarrantyTargetRule
{
    public List<string> Check(AdaptiveWarrantyTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyTargetRequiredLabelRule(string Message) : IAdaptiveWarrantyTargetRule
{
    public List<string> Check(AdaptiveWarrantyTarget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyTargetRequiredTitleRule(string Message) : IAdaptiveWarrantyTargetRule
{
    public List<string> Check(AdaptiveWarrantyTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyTargetPinnedGuardRule(int Limit) : IAdaptiveWarrantyTargetRule
{
    public List<string> Check(AdaptiveWarrantyTarget item)
    {
        if (item.Pinned && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyTargetRequiredDescriptionRule(string Message) : IAdaptiveWarrantyTargetRule
{
    public List<string> Check(AdaptiveWarrantyTarget item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive warranty target.</summary>
public sealed record AdaptiveWarrantyTargetValidator(List<IAdaptiveWarrantyTargetRule> Rules)
{
    public List<string> Validate(AdaptiveWarrantyTarget item)
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

    public void RequireValid(AdaptiveWarrantyTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveWarrantyTarget> items)
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

/// <summary>Functions of adaptive warranty target rules.</summary>
public static class AdaptiveWarrantyTargetRules
{
    public static AdaptiveWarrantyTargetValidator DefaultAdaptiveWarrantyTargetValidator()
    {
        return new AdaptiveWarrantyTargetValidator(
            new List<IAdaptiveWarrantyTargetRule>
            {
                new AdaptiveWarrantyTargetDescriptionLengthRule(1000),
                new AdaptiveWarrantyTargetCodeLengthRule(80),
                new AdaptiveWarrantyTargetRequiredLabelRule("adaptive warranty target needs a value"),
                new AdaptiveWarrantyTargetRequiredTitleRule("adaptive warranty target needs this field"),
                new AdaptiveWarrantyTargetPinnedGuardRule(1000),
                new AdaptiveWarrantyTargetRequiredDescriptionRule("adaptive warranty target needs an entry"),
            });
    }
}
