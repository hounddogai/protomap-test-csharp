namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a succession plan reservation must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanReservationRule
{
    List<string> Check(SuccessionPlanReservation item);
}

public sealed record SuccessionPlanReservationAllowedContactEmailRule(List<string> Allowed) : ISuccessionPlanReservationRule
{
    public List<string> Check(SuccessionPlanReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is expired" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanReservationRequiredNameRule(string Message) : ISuccessionPlanReservationRule
{
    public List<string> Check(SuccessionPlanReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanReservationRequiredTeamRule(string Message) : ISuccessionPlanReservationRule
{
    public List<string> Check(SuccessionPlanReservation item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan reservation.</summary>
public sealed record SuccessionPlanReservationValidator(List<ISuccessionPlanReservationRule> Rules)
{
    public List<string> Validate(SuccessionPlanReservation item)
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

    public void RequireValid(SuccessionPlanReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanReservation> items)
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

/// <summary>Functions of succession plan reservation rules.</summary>
public static class SuccessionPlanReservationRules
{
    public static SuccessionPlanReservationValidator DefaultSuccessionPlanReservationValidator()
    {
        return new SuccessionPlanReservationValidator(
            new List<ISuccessionPlanReservationRule>
            {
                new SuccessionPlanReservationAllowedContactEmailRule(new List<string> { "batched", "default" }),
                new SuccessionPlanReservationRequiredNameRule("succession plan reservation needs an entry"),
                new SuccessionPlanReservationRequiredTeamRule("succession plan reservation needs an entry"),
            });
    }
}
