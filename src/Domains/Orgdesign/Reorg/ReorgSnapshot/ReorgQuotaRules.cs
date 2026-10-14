namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg quota must pass. Check returns its problems, or none.</summary>
public interface IReorgQuotaRule
{
    List<string> Check(ReorgQuota item);
}

public sealed record ReorgQuotaDiscountRangeRule(double Min, double Max) : IReorgQuotaRule
{
    public List<string> Check(ReorgQuota item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgQuotaRequiredChannelRule(string Message) : IReorgQuotaRule
{
    public List<string> Check(ReorgQuota item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgQuotaRequiredOwnerIdRule(string Message) : IReorgQuotaRule
{
    public List<string> Check(ReorgQuota item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgQuotaRequiredCodeRule(string Message) : IReorgQuotaRule
{
    public List<string> Check(ReorgQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgQuotaAllowedCodeRule(List<string> Allowed) : IReorgQuotaRule
{
    public List<string> Check(ReorgQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record ReorgQuotaCodeLengthRule(int MaxLength) : IReorgQuotaRule
{
    public List<string> Check(ReorgQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg quota.</summary>
public sealed record ReorgQuotaValidator(List<IReorgQuotaRule> Rules)
{
    public List<string> Validate(ReorgQuota item)
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

    public void RequireValid(ReorgQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgQuota> items)
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

/// <summary>Functions of reorg quota rules.</summary>
public static class ReorgQuotaRules
{
    public static ReorgQuotaValidator DefaultReorgQuotaValidator()
    {
        return new ReorgQuotaValidator(
            new List<IReorgQuotaRule>
            {
                new ReorgQuotaDiscountRangeRule(8.0, 1478.9),
                new ReorgQuotaRequiredChannelRule("reorg quota needs a value"),
                new ReorgQuotaRequiredOwnerIdRule("reorg quota needs a value"),
                new ReorgQuotaRequiredCodeRule("reorg quota needs a value"),
                new ReorgQuotaAllowedCodeRule(new List<string> { "rolling" }),
                new ReorgQuotaCodeLengthRule(80),
            });
    }
}
