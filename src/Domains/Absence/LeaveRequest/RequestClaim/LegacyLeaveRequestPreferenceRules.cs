namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>A rule a legacy leave request preference must pass. Check returns its problems, or none.</summary>
public interface ILegacyLeaveRequestPreferenceRule
{
    List<string> Check(LegacyLeaveRequestPreference item);
}

public sealed record LegacyLeaveRequestPreferenceRequiredLabelRule(string Message) : ILegacyLeaveRequestPreferenceRule
{
    public List<string> Check(LegacyLeaveRequestPreference item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyLeaveRequestPreferenceCodePrefixRule(string Prefix) : ILegacyLeaveRequestPreferenceRule
{
    public List<string> Check(LegacyLeaveRequestPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLeaveRequestPreferenceRequiredTitleRule(string Message) : ILegacyLeaveRequestPreferenceRule
{
    public List<string> Check(LegacyLeaveRequestPreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyLeaveRequestPreferenceMinimumRangeRule(double Min, double Max) : ILegacyLeaveRequestPreferenceRule
{
    public List<string> Check(LegacyLeaveRequestPreference item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a legacy leave request preference.</summary>
public sealed record LegacyLeaveRequestPreferenceValidator(List<ILegacyLeaveRequestPreferenceRule> Rules)
{
    public List<string> Validate(LegacyLeaveRequestPreference item)
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

    public void RequireValid(LegacyLeaveRequestPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyLeaveRequestPreference> items)
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

/// <summary>Functions of legacy leave request preference rules.</summary>
public static class LegacyLeaveRequestPreferenceRules
{
    public static LegacyLeaveRequestPreferenceValidator DefaultLegacyLeaveRequestPreferenceValidator()
    {
        return new LegacyLeaveRequestPreferenceValidator(
            new List<ILegacyLeaveRequestPreferenceRule>
            {
                new LegacyLeaveRequestPreferenceRequiredLabelRule("legacy leave request preference needs an entry"),
                new LegacyLeaveRequestPreferenceCodePrefixRule("OPS"),
                new LegacyLeaveRequestPreferenceRequiredTitleRule("legacy leave request preference needs an entry"),
                new LegacyLeaveRequestPreferenceMinimumRangeRule(3.1, 3654.7),
            });
    }
}
