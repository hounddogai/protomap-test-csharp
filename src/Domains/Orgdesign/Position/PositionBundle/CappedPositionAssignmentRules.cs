namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>A rule a capped position assignment must pass. Check returns its problems, or none.</summary>
public interface ICappedPositionAssignmentRule
{
    List<string> Check(CappedPositionAssignment item);
}

public sealed record CappedPositionAssignmentCodePrefixRule(string Prefix) : ICappedPositionAssignmentRule
{
    public List<string> Check(CappedPositionAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPositionAssignmentRateRangeRule(double Min, double Max) : ICappedPositionAssignmentRule
{
    public List<string> Check(CappedPositionAssignment item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPositionAssignmentTeamLengthRule(int MaxLength) : ICappedPositionAssignmentRule
{
    public List<string> Check(CappedPositionAssignment item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPositionAssignmentRequiredTeamRule(string Message) : ICappedPositionAssignmentRule
{
    public List<string> Check(CappedPositionAssignment item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPositionAssignmentVisibleGuardRule(double Limit) : ICappedPositionAssignmentRule
{
    public List<string> Check(CappedPositionAssignment item)
    {
        if (item.Visible && item.Ratio > Limit)
        {
            return new List<string> { $"ratio {item.Ratio} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped position assignment.</summary>
public sealed record CappedPositionAssignmentValidator(List<ICappedPositionAssignmentRule> Rules)
{
    public List<string> Validate(CappedPositionAssignment item)
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

    public void RequireValid(CappedPositionAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPositionAssignment> items)
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

/// <summary>Functions of capped position assignment rules.</summary>
public static class CappedPositionAssignmentRules
{
    public static CappedPositionAssignmentValidator DefaultCappedPositionAssignmentValidator()
    {
        return new CappedPositionAssignmentValidator(
            new List<ICappedPositionAssignmentRule>
            {
                new CappedPositionAssignmentCodePrefixRule("X"),
                new CappedPositionAssignmentRateRangeRule(9.3, 3065.1),
                new CappedPositionAssignmentTeamLengthRule(64),
                new CappedPositionAssignmentRequiredTeamRule("capped position assignment needs a value"),
                new CappedPositionAssignmentVisibleGuardRule(7.1),
            });
    }
}
