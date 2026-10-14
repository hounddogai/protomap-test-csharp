namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a desk claim must pass. Check returns its problems, or none.</summary>
public interface IDeskClaimRule
{
    List<string> Check(DeskClaim item);
}

public sealed record DeskClaimRequiredDepartmentRule(string Message) : IDeskClaimRule
{
    public List<string> Check(DeskClaim item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskClaimRequiredCodeRule(string Message) : IDeskClaimRule
{
    public List<string> Check(DeskClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskClaimAllowedLabelRule(List<string> Allowed) : IDeskClaimRule
{
    public List<string> Check(DeskClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record DeskClaimCodeLengthRule(int MaxLength) : IDeskClaimRule
{
    public List<string> Check(DeskClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskClaimLabelLengthRule(int MaxLength) : IDeskClaimRule
{
    public List<string> Check(DeskClaim item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk claim.</summary>
public sealed record DeskClaimValidator(List<IDeskClaimRule> Rules)
{
    public List<string> Validate(DeskClaim item)
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

    public void RequireValid(DeskClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskClaim> items)
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

/// <summary>Functions of desk claim rules.</summary>
public static class DeskClaimRules
{
    public static DeskClaimValidator DefaultDeskClaimValidator()
    {
        return new DeskClaimValidator(
            new List<IDeskClaimRule>
            {
                new DeskClaimRequiredDepartmentRule("desk claim needs a value"),
                new DeskClaimRequiredCodeRule("desk claim needs a value"),
                new DeskClaimAllowedLabelRule(new List<string> { "global", "capped", "seasonal" }),
                new DeskClaimCodeLengthRule(255),
                new DeskClaimLabelLengthRule(3600),
            });
    }
}
