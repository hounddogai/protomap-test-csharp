namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default vendor score must pass. Check returns its problems, or none.</summary>
public interface IDefaultVendorScoreRule
{
    List<string> Check(DefaultVendorScore item);
}

public sealed record DefaultVendorScoreRequiredJobCodeRule(string Message) : IDefaultVendorScoreRule
{
    public List<string> Check(DefaultVendorScore item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultVendorScoreAllowedCodeRule(List<string> Allowed) : IDefaultVendorScoreRule
{
    public List<string> Check(DefaultVendorScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record DefaultVendorScoreNameLengthRule(int MaxLength) : IDefaultVendorScoreRule
{
    public List<string> Check(DefaultVendorScore item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default vendor score.</summary>
public sealed record DefaultVendorScoreValidator(List<IDefaultVendorScoreRule> Rules)
{
    public List<string> Validate(DefaultVendorScore item)
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

    public void RequireValid(DefaultVendorScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultVendorScore> items)
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

/// <summary>Functions of default vendor score rules.</summary>
public static class DefaultVendorScoreRules
{
    public static DefaultVendorScoreValidator DefaultDefaultVendorScoreValidator()
    {
        return new DefaultVendorScoreValidator(
            new List<IDefaultVendorScoreRule>
            {
                new DefaultVendorScoreRequiredJobCodeRule("default vendor score needs an entry"),
                new DefaultVendorScoreAllowedCodeRule(new List<string> { "capped" }),
                new DefaultVendorScoreNameLengthRule(120),
            });
    }
}
