namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive merit cycle reservation must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveMeritCycleReservationRule
{
    List<string> Check(AdaptiveMeritCycleReservation item);
}

public sealed record AdaptiveMeritCycleReservationSkillLengthRule(int MaxLength) : IAdaptiveMeritCycleReservationRule
{
    public List<string> Check(AdaptiveMeritCycleReservation item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveMeritCycleReservationCodePrefixRule(string Prefix) : IAdaptiveMeritCycleReservationRule
{
    public List<string> Check(AdaptiveMeritCycleReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveMeritCycleReservationAllowedSkillRule(List<string> Allowed) : IAdaptiveMeritCycleReservationRule
{
    public List<string> Check(AdaptiveMeritCycleReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveMeritCycleReservationTitleLengthRule(int MaxLength) : IAdaptiveMeritCycleReservationRule
{
    public List<string> Check(AdaptiveMeritCycleReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive merit cycle reservation.</summary>
public sealed record AdaptiveMeritCycleReservationValidator(List<IAdaptiveMeritCycleReservationRule> Rules)
{
    public List<string> Validate(AdaptiveMeritCycleReservation item)
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

    public void RequireValid(AdaptiveMeritCycleReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveMeritCycleReservation> items)
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

/// <summary>Functions of adaptive merit cycle reservation rules.</summary>
public static class AdaptiveMeritCycleReservationRules
{
    public static AdaptiveMeritCycleReservationValidator DefaultAdaptiveMeritCycleReservationValidator()
    {
        return new AdaptiveMeritCycleReservationValidator(
            new List<IAdaptiveMeritCycleReservationRule>
            {
                new AdaptiveMeritCycleReservationSkillLengthRule(32),
                new AdaptiveMeritCycleReservationCodePrefixRule("REF"),
                new AdaptiveMeritCycleReservationAllowedSkillRule(new List<string> { "scheduled" }),
                new AdaptiveMeritCycleReservationTitleLengthRule(32),
            });
    }
}
