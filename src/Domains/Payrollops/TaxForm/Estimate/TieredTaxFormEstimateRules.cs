namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered tax form estimate must pass. Check returns its problems, or none.</summary>
public interface ITieredTaxFormEstimateRule
{
    List<string> Check(TieredTaxFormEstimate item);
}

public sealed record TieredTaxFormEstimateRequiredExternalRefRule(string Message) : ITieredTaxFormEstimateRule
{
    public List<string> Check(TieredTaxFormEstimate item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredTaxFormEstimateAllowedNameRule(List<string> Allowed) : ITieredTaxFormEstimateRule
{
    public List<string> Check(TieredTaxFormEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

public sealed record TieredTaxFormEstimateCodeLengthRule(int MaxLength) : ITieredTaxFormEstimateRule
{
    public List<string> Check(TieredTaxFormEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredTaxFormEstimateExternalRefLengthRule(int MaxLength) : ITieredTaxFormEstimateRule
{
    public List<string> Check(TieredTaxFormEstimate item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered tax form estimate.</summary>
public sealed record TieredTaxFormEstimateValidator(List<ITieredTaxFormEstimateRule> Rules)
{
    public List<string> Validate(TieredTaxFormEstimate item)
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

    public void RequireValid(TieredTaxFormEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredTaxFormEstimate> items)
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

/// <summary>Functions of tiered tax form estimate rules.</summary>
public static class TieredTaxFormEstimateRules
{
    public static TieredTaxFormEstimateValidator DefaultTieredTaxFormEstimateValidator()
    {
        return new TieredTaxFormEstimateValidator(
            new List<ITieredTaxFormEstimateRule>
            {
                new TieredTaxFormEstimateRequiredExternalRefRule("tiered tax form estimate needs an entry"),
                new TieredTaxFormEstimateAllowedNameRule(new List<string> { "global", "legacy", "fallback" }),
                new TieredTaxFormEstimateCodeLengthRule(64),
                new TieredTaxFormEstimateExternalRefLengthRule(3600),
            });
    }
}
