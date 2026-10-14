namespace Acme.Workforce.Payrollops.Deduction.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual deduction notice must pass. Check returns its problems, or none.</summary>
public interface IManualDeductionNoticeRule
{
    List<string> Check(ManualDeductionNotice item);
}

public sealed record ManualDeductionNoticeRequiredCodeRule(string Message) : IManualDeductionNoticeRule
{
    public List<string> Check(ManualDeductionNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualDeductionNoticeArchivedGuardRule(int Limit) : IManualDeductionNoticeRule
{
    public List<string> Check(ManualDeductionNotice item)
    {
        if (item.Archived && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record ManualDeductionNoticeAllowedUnitRule(List<string> Allowed) : IManualDeductionNoticeRule
{
    public List<string> Check(ManualDeductionNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is too long" };
        }
        return new List<string>();
    }
}

public sealed record ManualDeductionNoticeCodeLengthRule(int MaxLength) : IManualDeductionNoticeRule
{
    public List<string> Check(ManualDeductionNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual deduction notice.</summary>
public sealed record ManualDeductionNoticeValidator(List<IManualDeductionNoticeRule> Rules)
{
    public List<string> Validate(ManualDeductionNotice item)
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

    public void RequireValid(ManualDeductionNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualDeductionNotice> items)
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

/// <summary>Functions of manual deduction notice rules.</summary>
public static class ManualDeductionNoticeRules
{
    public static ManualDeductionNoticeValidator DefaultManualDeductionNoticeValidator()
    {
        return new ManualDeductionNoticeValidator(
            new List<IManualDeductionNoticeRule>
            {
                new ManualDeductionNoticeRequiredCodeRule("manual deduction notice needs an entry"),
                new ManualDeductionNoticeArchivedGuardRule(1000),
                new ManualDeductionNoticeAllowedUnitRule(new List<string> { "weighted" }),
                new ManualDeductionNoticeCodeLengthRule(1000),
            });
    }
}
