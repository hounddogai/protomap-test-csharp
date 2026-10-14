namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient leave request claim must pass. Check returns its problems, or none.</summary>
public interface ILenientLeaveRequestClaimRule
{
    List<string> Check(LenientLeaveRequestClaim item);
}

public sealed record LenientLeaveRequestClaimReviewerLengthRule(int MaxLength) : ILenientLeaveRequestClaimRule
{
    public List<string> Check(LenientLeaveRequestClaim item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLeaveRequestClaimAllowedPhoneRule(List<string> Allowed) : ILenientLeaveRequestClaimRule
{
    public List<string> Check(LenientLeaveRequestClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record LenientLeaveRequestClaimCodePrefixRule(string Prefix) : ILenientLeaveRequestClaimRule
{
    public List<string> Check(LenientLeaveRequestClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLeaveRequestClaimRequiredEmployeeIdRule(string Message) : ILenientLeaveRequestClaimRule
{
    public List<string> Check(LenientLeaveRequestClaim item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient leave request claim.</summary>
public sealed record LenientLeaveRequestClaimValidator(List<ILenientLeaveRequestClaimRule> Rules)
{
    public List<string> Validate(LenientLeaveRequestClaim item)
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

    public void RequireValid(LenientLeaveRequestClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientLeaveRequestClaim> items)
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

/// <summary>Functions of lenient leave request claim rules.</summary>
public static class LenientLeaveRequestClaimRules
{
    public static LenientLeaveRequestClaimValidator DefaultLenientLeaveRequestClaimValidator()
    {
        return new LenientLeaveRequestClaimValidator(
            new List<ILenientLeaveRequestClaimRule>
            {
                new LenientLeaveRequestClaimReviewerLengthRule(16),
                new LenientLeaveRequestClaimAllowedPhoneRule(new List<string> { "strict" }),
                new LenientLeaveRequestClaimCodePrefixRule("OPS"),
                new LenientLeaveRequestClaimRequiredEmployeeIdRule("lenient leave request claim needs an entry"),
            });
    }
}
