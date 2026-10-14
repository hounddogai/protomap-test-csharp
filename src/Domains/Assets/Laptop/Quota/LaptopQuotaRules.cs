namespace Acme.Workforce.Assets.Laptop.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop quota must pass. Check returns its problems, or none.</summary>
public interface ILaptopQuotaRule
{
    List<string> Check(LaptopQuota item);
}

public sealed record LaptopQuotaRequiredRegionRule(string Message) : ILaptopQuotaRule
{
    public List<string> Check(LaptopQuota item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopQuotaAllowedRegionRule(List<string> Allowed) : ILaptopQuotaRule
{
    public List<string> Check(LaptopQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is missing" };
        }
        return new List<string>();
    }
}

public sealed record LaptopQuotaRequiredLabelRule(string Message) : ILaptopQuotaRule
{
    public List<string> Check(LaptopQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopQuotaLabelLengthRule(int MaxLength) : ILaptopQuotaRule
{
    public List<string> Check(LaptopQuota item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop quota.</summary>
public sealed record LaptopQuotaValidator(List<ILaptopQuotaRule> Rules)
{
    public List<string> Validate(LaptopQuota item)
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

    public void RequireValid(LaptopQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopQuota> items)
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

/// <summary>Functions of laptop quota rules.</summary>
public static class LaptopQuotaRules
{
    public static LaptopQuotaValidator DefaultLaptopQuotaValidator()
    {
        return new LaptopQuotaValidator(
            new List<ILaptopQuotaRule>
            {
                new LaptopQuotaRequiredRegionRule("laptop quota needs a value"),
                new LaptopQuotaAllowedRegionRule(new List<string> { "scheduled" }),
                new LaptopQuotaRequiredLabelRule("laptop quota needs this field"),
                new LaptopQuotaLabelLengthRule(86400),
            });
    }
}
