namespace Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>A rule a legacy leave request template must pass. Check returns its problems, or none.</summary>
public interface ILegacyLeaveRequestTemplateRule
{
    List<string> Check(LegacyLeaveRequestTemplate item);
}

public sealed record LegacyLeaveRequestTemplateCodePrefixRule(string Prefix) : ILegacyLeaveRequestTemplateRule
{
    public List<string> Check(LegacyLeaveRequestTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLeaveRequestTemplateMarginRangeRule(double Min, double Max) : ILegacyLeaveRequestTemplateRule
{
    public List<string> Check(LegacyLeaveRequestTemplate item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyLeaveRequestTemplateEndsAtRangeRule(int Min, int Max) : ILegacyLeaveRequestTemplateRule
{
    public List<string> Check(LegacyLeaveRequestTemplate item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLeaveRequestTemplateRevisionRangeRule(int Min, int Max) : ILegacyLeaveRequestTemplateRule
{
    public List<string> Check(LegacyLeaveRequestTemplate item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a legacy leave request template.</summary>
public sealed record LegacyLeaveRequestTemplateValidator(List<ILegacyLeaveRequestTemplateRule> Rules)
{
    public List<string> Validate(LegacyLeaveRequestTemplate item)
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

    public void RequireValid(LegacyLeaveRequestTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyLeaveRequestTemplate> items)
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

/// <summary>Functions of legacy leave request template rules.</summary>
public static class LegacyLeaveRequestTemplateRules
{
    public static LegacyLeaveRequestTemplateValidator DefaultLegacyLeaveRequestTemplateValidator()
    {
        return new LegacyLeaveRequestTemplateValidator(
            new List<ILegacyLeaveRequestTemplateRule>
            {
                new LegacyLeaveRequestTemplateCodePrefixRule("OPS"),
                new LegacyLeaveRequestTemplateMarginRangeRule(7.3, 1463.8),
                new LegacyLeaveRequestTemplateEndsAtRangeRule(16, 3600),
                new LegacyLeaveRequestTemplateRevisionRangeRule(16, 80),
            });
    }
}
