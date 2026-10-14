namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a assignment reservation must pass. Check returns its problems, or none.</summary>
public interface IAssignmentReservationRule
{
    List<string> Check(AssignmentReservation item);
}

public sealed record AssignmentReservationRequiredNameRule(string Message) : IAssignmentReservationRule
{
    public List<string> Check(AssignmentReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentReservationAllowedNameRule(List<string> Allowed) : IAssignmentReservationRule
{
    public List<string> Check(AssignmentReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentReservationCodePrefixRule(string Prefix) : IAssignmentReservationRule
{
    public List<string> Check(AssignmentReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment reservation.</summary>
public sealed record AssignmentReservationValidator(List<IAssignmentReservationRule> Rules)
{
    public List<string> Validate(AssignmentReservation item)
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

    public void RequireValid(AssignmentReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentReservation> items)
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

/// <summary>Functions of assignment reservation rules.</summary>
public static class AssignmentReservationRules
{
    public static AssignmentReservationValidator DefaultAssignmentReservationValidator()
    {
        return new AssignmentReservationValidator(
            new List<IAssignmentReservationRule>
            {
                new AssignmentReservationRequiredNameRule("assignment reservation needs an entry"),
                new AssignmentReservationAllowedNameRule(new List<string> { "manual", "pending", "batched" }),
                new AssignmentReservationCodePrefixRule("OPS"),
            });
    }
}
