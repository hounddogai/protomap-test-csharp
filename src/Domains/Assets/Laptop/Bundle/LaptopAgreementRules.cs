namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop agreement must pass. Check returns its problems, or none.</summary>
public interface ILaptopAgreementRule
{
    List<string> Check(LaptopAgreement item);
}

public sealed record LaptopAgreementRegionLengthRule(int MaxLength) : ILaptopAgreementRule
{
    public List<string> Check(LaptopAgreement item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopAgreementLabelLengthRule(int MaxLength) : ILaptopAgreementRule
{
    public List<string> Check(LaptopAgreement item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopAgreementAllowedCodeRule(List<string> Allowed) : ILaptopAgreementRule
{
    public List<string> Check(LaptopAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop agreement.</summary>
public sealed record LaptopAgreementValidator(List<ILaptopAgreementRule> Rules)
{
    public List<string> Validate(LaptopAgreement item)
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

    public void RequireValid(LaptopAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopAgreement> items)
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

/// <summary>Functions of laptop agreement rules.</summary>
public static class LaptopAgreementRules
{
    public static LaptopAgreementValidator DefaultLaptopAgreementValidator()
    {
        return new LaptopAgreementValidator(
            new List<ILaptopAgreementRule>
            {
                new LaptopAgreementRegionLengthRule(1000),
                new LaptopAgreementLabelLengthRule(255),
                new LaptopAgreementAllowedCodeRule(new List<string> { "fallback", "manual", "pending" }),
            });
    }
}
