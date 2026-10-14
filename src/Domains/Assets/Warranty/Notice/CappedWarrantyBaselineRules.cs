namespace Acme.Workforce.Assets.Warranty.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped warranty baseline must pass. Check returns its problems, or none.</summary>
public interface ICappedWarrantyBaselineRule
{
    List<string> Check(CappedWarrantyBaseline item);
}

public sealed record CappedWarrantyBaselineAllowedRegionRule(List<string> Allowed) : ICappedWarrantyBaselineRule
{
    public List<string> Check(CappedWarrantyBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record CappedWarrantyBaselineRequiredLabelRule(string Message) : ICappedWarrantyBaselineRule
{
    public List<string> Check(CappedWarrantyBaseline item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedWarrantyBaselineRequiredCodeRule(string Message) : ICappedWarrantyBaselineRule
{
    public List<string> Check(CappedWarrantyBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped warranty baseline.</summary>
public sealed record CappedWarrantyBaselineValidator(List<ICappedWarrantyBaselineRule> Rules)
{
    public List<string> Validate(CappedWarrantyBaseline item)
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

    public void RequireValid(CappedWarrantyBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedWarrantyBaseline> items)
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

/// <summary>Functions of capped warranty baseline rules.</summary>
public static class CappedWarrantyBaselineRules
{
    public static CappedWarrantyBaselineValidator DefaultCappedWarrantyBaselineValidator()
    {
        return new CappedWarrantyBaselineValidator(
            new List<ICappedWarrantyBaselineRule>
            {
                new CappedWarrantyBaselineAllowedRegionRule(new List<string> { "manual", "weighted", "tiered" }),
                new CappedWarrantyBaselineRequiredLabelRule("capped warranty baseline needs a value"),
                new CappedWarrantyBaselineRequiredCodeRule("capped warranty baseline needs this field"),
            });
    }
}
