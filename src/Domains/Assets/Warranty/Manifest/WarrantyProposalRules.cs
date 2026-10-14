namespace Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>A rule a warranty proposal must pass. Check returns its problems, or none.</summary>
public interface IWarrantyProposalRule
{
    List<string> Check(WarrantyProposal item);
}

public sealed record WarrantyProposalRequiredBadgeRule(string Message) : IWarrantyProposalRule
{
    public List<string> Check(WarrantyProposal item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyProposalCategoryLengthRule(int MaxLength) : IWarrantyProposalRule
{
    public List<string> Check(WarrantyProposal item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyProposalRatioRangeRule(double Min, double Max) : IWarrantyProposalRule
{
    public List<string> Check(WarrantyProposal item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty proposal.</summary>
public sealed record WarrantyProposalValidator(List<IWarrantyProposalRule> Rules)
{
    public List<string> Validate(WarrantyProposal item)
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

    public void RequireValid(WarrantyProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyProposal> items)
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

/// <summary>Functions of warranty proposal rules.</summary>
public static class WarrantyProposalRules
{
    public static WarrantyProposalValidator DefaultWarrantyProposalValidator()
    {
        return new WarrantyProposalValidator(
            new List<IWarrantyProposalRule>
            {
                new WarrantyProposalRequiredBadgeRule("warranty proposal needs an entry"),
                new WarrantyProposalCategoryLengthRule(120),
                new WarrantyProposalRatioRangeRule(6.1, 1931.7),
            });
    }
}
