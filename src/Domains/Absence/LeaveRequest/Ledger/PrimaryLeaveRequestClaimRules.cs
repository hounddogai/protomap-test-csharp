namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>A rule a primary leave request claim must pass. Check returns its problems, or none.</summary>
public interface IPrimaryLeaveRequestClaimRule
{
    List<string> Check(PrimaryLeaveRequestClaim item);
}

public sealed record PrimaryLeaveRequestClaimBatchSizeRangeRule(int Min, int Max) : IPrimaryLeaveRequestClaimRule
{
    public List<string> Check(PrimaryLeaveRequestClaim item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestClaimRequiredCodeRule(string Message) : IPrimaryLeaveRequestClaimRule
{
    public List<string> Check(PrimaryLeaveRequestClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestClaimRequiredSkillRule(string Message) : IPrimaryLeaveRequestClaimRule
{
    public List<string> Check(PrimaryLeaveRequestClaim item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestClaimNameLengthRule(int MaxLength) : IPrimaryLeaveRequestClaimRule
{
    public List<string> Check(PrimaryLeaveRequestClaim item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestClaimSkillLengthRule(int MaxLength) : IPrimaryLeaveRequestClaimRule
{
    public List<string> Check(PrimaryLeaveRequestClaim item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary leave request claim.</summary>
public sealed record PrimaryLeaveRequestClaimValidator(List<IPrimaryLeaveRequestClaimRule> Rules)
{
    public List<string> Validate(PrimaryLeaveRequestClaim item)
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

    public void RequireValid(PrimaryLeaveRequestClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryLeaveRequestClaim> items)
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

/// <summary>Functions of primary leave request claim rules.</summary>
public static class PrimaryLeaveRequestClaimRules
{
    public static PrimaryLeaveRequestClaimValidator DefaultPrimaryLeaveRequestClaimValidator()
    {
        return new PrimaryLeaveRequestClaimValidator(
            new List<IPrimaryLeaveRequestClaimRule>
            {
                new PrimaryLeaveRequestClaimBatchSizeRangeRule(16, 64),
                new PrimaryLeaveRequestClaimRequiredCodeRule("primary leave request claim needs an entry"),
                new PrimaryLeaveRequestClaimRequiredSkillRule("primary leave request claim needs a value"),
                new PrimaryLeaveRequestClaimNameLengthRule(3600),
                new PrimaryLeaveRequestClaimSkillLengthRule(1000),
            });
    }
}
