namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

/// <summary>A rule a return plan claim must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanClaimRule
{
    List<string> Check(ReturnPlanClaim item);
}

public sealed record ReturnPlanClaimRequiredSegmentRule(string Message) : IReturnPlanClaimRule
{
    public List<string> Check(ReturnPlanClaim item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanClaimCodePrefixRule(string Prefix) : IReturnPlanClaimRule
{
    public List<string> Check(ReturnPlanClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanClaimArchivedGuardRule(double Limit) : IReturnPlanClaimRule
{
    public List<string> Check(ReturnPlanClaim item)
    {
        if (item.Archived && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanClaimRequiredChecksumRule(string Message) : IReturnPlanClaimRule
{
    public List<string> Check(ReturnPlanClaim item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanClaimSegmentLengthRule(int MaxLength) : IReturnPlanClaimRule
{
    public List<string> Check(ReturnPlanClaim item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanClaimRequiredCodeRule(string Message) : IReturnPlanClaimRule
{
    public List<string> Check(ReturnPlanClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan claim.</summary>
public sealed record ReturnPlanClaimValidator(List<IReturnPlanClaimRule> Rules)
{
    public List<string> Validate(ReturnPlanClaim item)
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

    public void RequireValid(ReturnPlanClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanClaim> items)
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

/// <summary>Functions of return plan claim rules.</summary>
public static class ReturnPlanClaimRules
{
    public static ReturnPlanClaimValidator DefaultReturnPlanClaimValidator()
    {
        return new ReturnPlanClaimValidator(
            new List<IReturnPlanClaimRule>
            {
                new ReturnPlanClaimRequiredSegmentRule("return plan claim needs this field"),
                new ReturnPlanClaimCodePrefixRule("ACM"),
                new ReturnPlanClaimArchivedGuardRule(1.5),
                new ReturnPlanClaimRequiredChecksumRule("return plan claim needs a value"),
                new ReturnPlanClaimSegmentLengthRule(255),
                new ReturnPlanClaimRequiredCodeRule("return plan claim needs an entry"),
            });
    }
}
