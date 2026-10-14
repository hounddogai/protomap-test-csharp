namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped tax form rule must pass. Check returns its problems, or none.</summary>
public interface ICappedTaxFormRuleRule
{
    List<string> Check(CappedTaxFormRule item);
}

public sealed record CappedTaxFormRuleNameLengthRule(int MaxLength) : ICappedTaxFormRuleRule
{
    public List<string> Check(CappedTaxFormRule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedTaxFormRuleChecksumLengthRule(int MaxLength) : ICappedTaxFormRuleRule
{
    public List<string> Check(CappedTaxFormRule item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedTaxFormRuleAllowedNameRule(List<string> Allowed) : ICappedTaxFormRuleRule
{
    public List<string> Check(CappedTaxFormRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped tax form rule.</summary>
public sealed record CappedTaxFormRuleValidator(List<ICappedTaxFormRuleRule> Rules)
{
    public List<string> Validate(CappedTaxFormRule item)
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

    public void RequireValid(CappedTaxFormRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedTaxFormRule> items)
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

/// <summary>Functions of capped tax form rule rules.</summary>
public static class CappedTaxFormRuleRules
{
    public static CappedTaxFormRuleValidator DefaultCappedTaxFormRuleValidator()
    {
        return new CappedTaxFormRuleValidator(
            new List<ICappedTaxFormRuleRule>
            {
                new CappedTaxFormRuleNameLengthRule(32),
                new CappedTaxFormRuleChecksumLengthRule(3600),
                new CappedTaxFormRuleAllowedNameRule(new List<string> { "adaptive", "tiered", "rolling" }),
            });
    }
}
