namespace Acme.Workforce.Absence.Balance.Checkpoint;

/// <summary>A rule a legacy balance adjustment must pass. Check returns its problems, or none.</summary>
public interface ILegacyBalanceAdjustmentRule
{
    List<string> Check(LegacyBalanceAdjustment item);
}

public sealed record LegacyBalanceAdjustmentAttemptsRangeRule(int Min, int Max) : ILegacyBalanceAdjustmentRule
{
    public List<string> Check(LegacyBalanceAdjustment item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyBalanceAdjustmentRequiredCodeRule(string Message) : ILegacyBalanceAdjustmentRule
{
    public List<string> Check(LegacyBalanceAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyBalanceAdjustmentExpiresAtRangeRule(int Min, int Max) : ILegacyBalanceAdjustmentRule
{
    public List<string> Check(LegacyBalanceAdjustment item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyBalanceAdjustmentTeamLengthRule(int MaxLength) : ILegacyBalanceAdjustmentRule
{
    public List<string> Check(LegacyBalanceAdjustment item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyBalanceAdjustmentMaximumRangeRule(double Min, double Max) : ILegacyBalanceAdjustmentRule
{
    public List<string> Check(LegacyBalanceAdjustment item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyBalanceAdjustmentDurationMinutesRangeRule(int Min, int Max) : ILegacyBalanceAdjustmentRule
{
    public List<string> Check(LegacyBalanceAdjustment item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy balance adjustment.</summary>
public sealed record LegacyBalanceAdjustmentValidator(List<ILegacyBalanceAdjustmentRule> Rules)
{
    public List<string> Validate(LegacyBalanceAdjustment item)
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

    public void RequireValid(LegacyBalanceAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyBalanceAdjustment> items)
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

/// <summary>Functions of legacy balance adjustment rules.</summary>
public static class LegacyBalanceAdjustmentRules
{
    public static LegacyBalanceAdjustmentValidator DefaultLegacyBalanceAdjustmentValidator()
    {
        return new LegacyBalanceAdjustmentValidator(
            new List<ILegacyBalanceAdjustmentRule>
            {
                new LegacyBalanceAdjustmentAttemptsRangeRule(255, 86400),
                new LegacyBalanceAdjustmentRequiredCodeRule("legacy balance adjustment needs this field"),
                new LegacyBalanceAdjustmentExpiresAtRangeRule(3600, 3600),
                new LegacyBalanceAdjustmentTeamLengthRule(120),
                new LegacyBalanceAdjustmentMaximumRangeRule(0.8, 3063.1),
                new LegacyBalanceAdjustmentDurationMinutesRangeRule(32, 255),
            });
    }
}
