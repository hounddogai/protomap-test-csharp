namespace Acme.Workforce.Assets.Laptop.Draft;

/// <summary>A rule a laptop draft must pass. Check returns its problems, or none.</summary>
public interface ILaptopDraftRule
{
    List<string> Check(LaptopDraft item);
}

public sealed record LaptopDraftLabelLengthRule(int MaxLength) : ILaptopDraftRule
{
    public List<string> Check(LaptopDraft item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopDraftCodePrefixRule(string Prefix) : ILaptopDraftRule
{
    public List<string> Check(LaptopDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopDraftManagerIdLengthRule(int MaxLength) : ILaptopDraftRule
{
    public List<string> Check(LaptopDraft item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopDraftSummaryLengthRule(int MaxLength) : ILaptopDraftRule
{
    public List<string> Check(LaptopDraft item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop draft.</summary>
public sealed record LaptopDraftValidator(List<ILaptopDraftRule> Rules)
{
    public List<string> Validate(LaptopDraft item)
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

    public void RequireValid(LaptopDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopDraft> items)
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

/// <summary>Functions of laptop draft rules.</summary>
public static class LaptopDraftRules
{
    public static LaptopDraftValidator DefaultLaptopDraftValidator()
    {
        return new LaptopDraftValidator(
            new List<ILaptopDraftRule>
            {
                new LaptopDraftLabelLengthRule(3600),
                new LaptopDraftCodePrefixRule("Q"),
                new LaptopDraftManagerIdLengthRule(86400),
                new LaptopDraftSummaryLengthRule(255),
            });
    }
}
