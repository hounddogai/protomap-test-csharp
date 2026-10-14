namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending pay band digest must pass. Check returns its problems, or none.</summary>
public interface IPendingPayBandDigestRule
{
    List<string> Check(PendingPayBandDigest item);
}

public sealed record PendingPayBandDigestRequiredLabelRule(string Message) : IPendingPayBandDigestRule
{
    public List<string> Check(PendingPayBandDigest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPayBandDigestAllowedChannelRule(List<string> Allowed) : IPendingPayBandDigestRule
{
    public List<string> Check(PendingPayBandDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record PendingPayBandDigestNameLengthRule(int MaxLength) : IPendingPayBandDigestRule
{
    public List<string> Check(PendingPayBandDigest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending pay band digest.</summary>
public sealed record PendingPayBandDigestValidator(List<IPendingPayBandDigestRule> Rules)
{
    public List<string> Validate(PendingPayBandDigest item)
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

    public void RequireValid(PendingPayBandDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPayBandDigest> items)
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

/// <summary>Functions of pending pay band digest rules.</summary>
public static class PendingPayBandDigestRules
{
    public static PendingPayBandDigestValidator DefaultPendingPayBandDigestValidator()
    {
        return new PendingPayBandDigestValidator(
            new List<IPendingPayBandDigestRule>
            {
                new PendingPayBandDigestRequiredLabelRule("pending pay band digest needs this field"),
                new PendingPayBandDigestAllowedChannelRule(new List<string> { "tiered" }),
                new PendingPayBandDigestNameLengthRule(120),
            });
    }
}
