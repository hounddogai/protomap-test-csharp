namespace Acme.Workforce.Talent.SkillProfile.ProfileGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling skill profile reservation must pass. Check returns its problems, or none.</summary>
public interface IRollingSkillProfileReservationRule
{
    List<string> Check(RollingSkillProfileReservation item);
}

public sealed record RollingSkillProfileReservationCostRangeRule(double Min, double Max) : IRollingSkillProfileReservationRule
{
    public List<string> Check(RollingSkillProfileReservation item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingSkillProfileReservationCodePrefixRule(string Prefix) : IRollingSkillProfileReservationRule
{
    public List<string> Check(RollingSkillProfileReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSkillProfileReservationRequiredCodeRule(string Message) : IRollingSkillProfileReservationRule
{
    public List<string> Check(RollingSkillProfileReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingSkillProfileReservationPinnedGuardRule(double Limit) : IRollingSkillProfileReservationRule
{
    public List<string> Check(RollingSkillProfileReservation item)
    {
        if (item.Pinned && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record RollingSkillProfileReservationLabelLengthRule(int MaxLength) : IRollingSkillProfileReservationRule
{
    public List<string> Check(RollingSkillProfileReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSkillProfileReservationAllowedLabelRule(List<string> Allowed) : IRollingSkillProfileReservationRule
{
    public List<string> Check(RollingSkillProfileReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling skill profile reservation.</summary>
public sealed record RollingSkillProfileReservationValidator(List<IRollingSkillProfileReservationRule> Rules)
{
    public List<string> Validate(RollingSkillProfileReservation item)
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

    public void RequireValid(RollingSkillProfileReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingSkillProfileReservation> items)
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

/// <summary>Functions of rolling skill profile reservation rules.</summary>
public static class RollingSkillProfileReservationRules
{
    public static RollingSkillProfileReservationValidator DefaultRollingSkillProfileReservationValidator()
    {
        return new RollingSkillProfileReservationValidator(
            new List<IRollingSkillProfileReservationRule>
            {
                new RollingSkillProfileReservationCostRangeRule(4.0, 4856.2),
                new RollingSkillProfileReservationCodePrefixRule("X"),
                new RollingSkillProfileReservationRequiredCodeRule("rolling skill profile reservation needs a value"),
                new RollingSkillProfileReservationPinnedGuardRule(2.3),
                new RollingSkillProfileReservationLabelLengthRule(32),
                new RollingSkillProfileReservationAllowedLabelRule(
                    new List<string> { "scheduled", "manual", "fallback" }),
            });
    }
}
