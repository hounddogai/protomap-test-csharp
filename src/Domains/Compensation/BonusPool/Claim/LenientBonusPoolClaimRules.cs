namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient bonus pool claim must pass. Check returns its problems, or none.</summary>
public interface ILenientBonusPoolClaimRule
{
    List<string> Check(LenientBonusPoolClaim item);
}

public sealed record LenientBonusPoolClaimJobCodeLengthRule(int MaxLength) : ILenientBonusPoolClaimRule
{
    public List<string> Check(LenientBonusPoolClaim item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolClaimRequiredNameRule(string Message) : ILenientBonusPoolClaimRule
{
    public List<string> Check(LenientBonusPoolClaim item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolClaimRequiredLocationRule(string Message) : ILenientBonusPoolClaimRule
{
    public List<string> Check(LenientBonusPoolClaim item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolClaimRequiredCodeRule(string Message) : ILenientBonusPoolClaimRule
{
    public List<string> Check(LenientBonusPoolClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolClaimNameLengthRule(int MaxLength) : ILenientBonusPoolClaimRule
{
    public List<string> Check(LenientBonusPoolClaim item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolClaimAllowedJobCodeRule(List<string> Allowed) : ILenientBonusPoolClaimRule
{
    public List<string> Check(LenientBonusPoolClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient bonus pool claim.</summary>
public sealed record LenientBonusPoolClaimValidator(List<ILenientBonusPoolClaimRule> Rules)
{
    public List<string> Validate(LenientBonusPoolClaim item)
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

    public void RequireValid(LenientBonusPoolClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBonusPoolClaim> items)
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

/// <summary>Functions of lenient bonus pool claim rules.</summary>
public static class LenientBonusPoolClaimRules
{
    public static LenientBonusPoolClaimValidator DefaultLenientBonusPoolClaimValidator()
    {
        return new LenientBonusPoolClaimValidator(
            new List<ILenientBonusPoolClaimRule>
            {
                new LenientBonusPoolClaimJobCodeLengthRule(255),
                new LenientBonusPoolClaimRequiredNameRule("lenient bonus pool claim needs this field"),
                new LenientBonusPoolClaimRequiredLocationRule("lenient bonus pool claim needs a value"),
                new LenientBonusPoolClaimRequiredCodeRule("lenient bonus pool claim needs a value"),
                new LenientBonusPoolClaimNameLengthRule(255),
                new LenientBonusPoolClaimAllowedJobCodeRule(new List<string> { "seasonal", "seasonal", "scheduled" }),
            });
    }
}
