namespace Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>A rule a lenient position audit must pass. Check returns its problems, or none.</summary>
public interface ILenientPositionAuditRule
{
    List<string> Check(LenientPositionAudit item);
}

public sealed record LenientPositionAuditDurationMinutesRangeRule(int Min, int Max) : ILenientPositionAuditRule
{
    public List<string> Check(LenientPositionAudit item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPositionAuditVisibleGuardRule(int Limit) : ILenientPositionAuditRule
{
    public List<string> Check(LenientPositionAudit item)
    {
        if (item.Visible && item.DurationMinutes > Limit)
        {
            return new List<string> { $"duration minutes {item.DurationMinutes} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record LenientPositionAuditCodePrefixRule(string Prefix) : ILenientPositionAuditRule
{
    public List<string> Check(LenientPositionAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient position audit.</summary>
public sealed record LenientPositionAuditValidator(List<ILenientPositionAuditRule> Rules)
{
    public List<string> Validate(LenientPositionAudit item)
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

    public void RequireValid(LenientPositionAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPositionAudit> items)
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

/// <summary>Functions of lenient position audit rules.</summary>
public static class LenientPositionAuditRules
{
    public static LenientPositionAuditValidator DefaultLenientPositionAuditValidator()
    {
        return new LenientPositionAuditValidator(
            new List<ILenientPositionAuditRule>
            {
                new LenientPositionAuditDurationMinutesRangeRule(16, 32),
                new LenientPositionAuditVisibleGuardRule(3600),
                new LenientPositionAuditCodePrefixRule("Q"),
            });
    }
}
