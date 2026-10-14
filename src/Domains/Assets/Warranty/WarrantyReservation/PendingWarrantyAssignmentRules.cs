namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending warranty assignment must pass. Check returns its problems, or none.</summary>
public interface IPendingWarrantyAssignmentRule
{
    List<string> Check(PendingWarrantyAssignment item);
}

public sealed record PendingWarrantyAssignmentAllowedCodeRule(List<string> Allowed) : IPendingWarrantyAssignmentRule
{
    public List<string> Check(PendingWarrantyAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record PendingWarrantyAssignmentRequiredGradeRule(string Message) : IPendingWarrantyAssignmentRule
{
    public List<string> Check(PendingWarrantyAssignment item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingWarrantyAssignmentAmountRangeRule(double Min, double Max) : IPendingWarrantyAssignmentRule
{
    public List<string> Check(PendingWarrantyAssignment item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending warranty assignment.</summary>
public sealed record PendingWarrantyAssignmentValidator(List<IPendingWarrantyAssignmentRule> Rules)
{
    public List<string> Validate(PendingWarrantyAssignment item)
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

    public void RequireValid(PendingWarrantyAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingWarrantyAssignment> items)
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

/// <summary>Functions of pending warranty assignment rules.</summary>
public static class PendingWarrantyAssignmentRules
{
    public static PendingWarrantyAssignmentValidator DefaultPendingWarrantyAssignmentValidator()
    {
        return new PendingWarrantyAssignmentValidator(
            new List<IPendingWarrantyAssignmentRule>
            {
                new PendingWarrantyAssignmentAllowedCodeRule(new List<string> { "adaptive", "legacy" }),
                new PendingWarrantyAssignmentRequiredGradeRule("pending warranty assignment needs a value"),
                new PendingWarrantyAssignmentAmountRangeRule(9.3, 2967.2),
            });
    }
}
