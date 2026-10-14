namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a cost model bundle must pass. Check returns its problems, or none.</summary>
public interface ICostModelBundleRule
{
    List<string> Check(CostModelBundle item);
}

public sealed record CostModelBundleCapacityRangeRule(int Min, int Max) : ICostModelBundleRule
{
    public List<string> Check(CostModelBundle item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelBundleRequiredCodeRule(string Message) : ICostModelBundleRule
{
    public List<string> Check(CostModelBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelBundleRequiredLabelRule(string Message) : ICostModelBundleRule
{
    public List<string> Check(CostModelBundle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelBundleAllowedLabelRule(List<string> Allowed) : ICostModelBundleRule
{
    public List<string> Check(CostModelBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record CostModelBundleLabelLengthRule(int MaxLength) : ICostModelBundleRule
{
    public List<string> Check(CostModelBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelBundleCodePrefixRule(string Prefix) : ICostModelBundleRule
{
    public List<string> Check(CostModelBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model bundle.</summary>
public sealed record CostModelBundleValidator(List<ICostModelBundleRule> Rules)
{
    public List<string> Validate(CostModelBundle item)
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

    public void RequireValid(CostModelBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelBundle> items)
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

/// <summary>Functions of cost model bundle rules.</summary>
public static class CostModelBundleRules
{
    public static CostModelBundleValidator DefaultCostModelBundleValidator()
    {
        return new CostModelBundleValidator(
            new List<ICostModelBundleRule>
            {
                new CostModelBundleCapacityRangeRule(64, 255),
                new CostModelBundleRequiredCodeRule("cost model bundle needs a value"),
                new CostModelBundleRequiredLabelRule("cost model bundle needs an entry"),
                new CostModelBundleAllowedLabelRule(new List<string> { "batched", "seasonal" }),
                new CostModelBundleLabelLengthRule(80),
                new CostModelBundleCodePrefixRule("OPS"),
            });
    }
}
