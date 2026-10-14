namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary equity grant digest must pass. Check returns its problems, or none.</summary>
public interface IPrimaryEquityGrantDigestRule
{
    List<string> Check(PrimaryEquityGrantDigest item);
}

public sealed record PrimaryEquityGrantDigestChannelLengthRule(int MaxLength) : IPrimaryEquityGrantDigestRule
{
    public List<string> Check(PrimaryEquityGrantDigest item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEquityGrantDigestRequiredCodeRule(string Message) : IPrimaryEquityGrantDigestRule
{
    public List<string> Check(PrimaryEquityGrantDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEquityGrantDigestAllowedCategoryRule(List<string> Allowed) : IPrimaryEquityGrantDigestRule
{
    public List<string> Check(PrimaryEquityGrantDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEquityGrantDigestNameLengthRule(int MaxLength) : IPrimaryEquityGrantDigestRule
{
    public List<string> Check(PrimaryEquityGrantDigest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary equity grant digest.</summary>
public sealed record PrimaryEquityGrantDigestValidator(List<IPrimaryEquityGrantDigestRule> Rules)
{
    public List<string> Validate(PrimaryEquityGrantDigest item)
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

    public void RequireValid(PrimaryEquityGrantDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryEquityGrantDigest> items)
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

/// <summary>Functions of primary equity grant digest rules.</summary>
public static class PrimaryEquityGrantDigestRules
{
    public static PrimaryEquityGrantDigestValidator DefaultPrimaryEquityGrantDigestValidator()
    {
        return new PrimaryEquityGrantDigestValidator(
            new List<IPrimaryEquityGrantDigestRule>
            {
                new PrimaryEquityGrantDigestChannelLengthRule(3600),
                new PrimaryEquityGrantDigestRequiredCodeRule("primary equity grant digest needs a value"),
                new PrimaryEquityGrantDigestAllowedCategoryRule(new List<string> { "lenient" }),
                new PrimaryEquityGrantDigestNameLengthRule(16),
            });
    }
}
