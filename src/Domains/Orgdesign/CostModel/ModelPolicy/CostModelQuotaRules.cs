namespace Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

/// <summary>A rule a cost model quota must pass. Check returns its problems, or none.</summary>
public interface ICostModelQuotaRule
{
    List<string> Check(CostModelQuota item);
}

public sealed record CostModelQuotaRequiredRegionRule(string Message) : ICostModelQuotaRule
{
    public List<string> Check(CostModelQuota item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelQuotaChecksumLengthRule(int MaxLength) : ICostModelQuotaRule
{
    public List<string> Check(CostModelQuota item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelQuotaRegionLengthRule(int MaxLength) : ICostModelQuotaRule
{
    public List<string> Check(CostModelQuota item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelQuotaCodeLengthRule(int MaxLength) : ICostModelQuotaRule
{
    public List<string> Check(CostModelQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model quota.</summary>
public sealed record CostModelQuotaValidator(List<ICostModelQuotaRule> Rules)
{
    public List<string> Validate(CostModelQuota item)
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

    public void RequireValid(CostModelQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelQuota> items)
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

/// <summary>Functions of cost model quota rules.</summary>
public static class CostModelQuotaRules
{
    public static CostModelQuotaValidator DefaultCostModelQuotaValidator()
    {
        return new CostModelQuotaValidator(
            new List<ICostModelQuotaRule>
            {
                new CostModelQuotaRequiredRegionRule("cost model quota needs this field"),
                new CostModelQuotaChecksumLengthRule(16),
                new CostModelQuotaRegionLengthRule(255),
                new CostModelQuotaCodeLengthRule(3600),
            });
    }
}
