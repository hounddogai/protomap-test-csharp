namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered warranty manifest must pass. Check returns its problems, or none.</summary>
public interface ITieredWarrantyManifestRule
{
    List<string> Check(TieredWarrantyManifest item);
}

public sealed record TieredWarrantyManifestRequiredCodeRule(string Message) : ITieredWarrantyManifestRule
{
    public List<string> Check(TieredWarrantyManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredWarrantyManifestAllowedContactEmailRule(List<string> Allowed) : ITieredWarrantyManifestRule
{
    public List<string> Check(TieredWarrantyManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record TieredWarrantyManifestRequiredCountryRule(string Message) : ITieredWarrantyManifestRule
{
    public List<string> Check(TieredWarrantyManifest item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered warranty manifest.</summary>
public sealed record TieredWarrantyManifestValidator(List<ITieredWarrantyManifestRule> Rules)
{
    public List<string> Validate(TieredWarrantyManifest item)
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

    public void RequireValid(TieredWarrantyManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredWarrantyManifest> items)
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

/// <summary>Functions of tiered warranty manifest rules.</summary>
public static class TieredWarrantyManifestRules
{
    public static TieredWarrantyManifestValidator DefaultTieredWarrantyManifestValidator()
    {
        return new TieredWarrantyManifestValidator(
            new List<ITieredWarrantyManifestRule>
            {
                new TieredWarrantyManifestRequiredCodeRule("tiered warranty manifest needs this field"),
                new TieredWarrantyManifestAllowedContactEmailRule(
                    new List<string> { "seasonal", "lenient", "weighted" }),
                new TieredWarrantyManifestRequiredCountryRule("tiered warranty manifest needs a value"),
            });
    }
}
