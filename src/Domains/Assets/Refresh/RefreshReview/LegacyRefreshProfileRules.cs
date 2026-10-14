namespace Acme.Workforce.Assets.Refresh.RefreshReview;

/// <summary>A rule a legacy refresh profile must pass. Check returns its problems, or none.</summary>
public interface ILegacyRefreshProfileRule
{
    List<string> Check(LegacyRefreshProfile item);
}

public sealed record LegacyRefreshProfileAttemptsRangeRule(int Min, int Max) : ILegacyRefreshProfileRule
{
    public List<string> Check(LegacyRefreshProfile item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyRefreshProfileTenureMonthsRangeRule(int Min, int Max) : ILegacyRefreshProfileRule
{
    public List<string> Check(LegacyRefreshProfile item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyRefreshProfileEmployeeIdLengthRule(int MaxLength) : ILegacyRefreshProfileRule
{
    public List<string> Check(LegacyRefreshProfile item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy refresh profile.</summary>
public sealed record LegacyRefreshProfileValidator(List<ILegacyRefreshProfileRule> Rules)
{
    public List<string> Validate(LegacyRefreshProfile item)
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

    public void RequireValid(LegacyRefreshProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyRefreshProfile> items)
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

/// <summary>Functions of legacy refresh profile rules.</summary>
public static class LegacyRefreshProfileRules
{
    public static LegacyRefreshProfileValidator DefaultLegacyRefreshProfileValidator()
    {
        return new LegacyRefreshProfileValidator(
            new List<ILegacyRefreshProfileRule>
            {
                new LegacyRefreshProfileAttemptsRangeRule(16, 64),
                new LegacyRefreshProfileTenureMonthsRangeRule(32, 3600),
                new LegacyRefreshProfileEmployeeIdLengthRule(3600),
            });
    }
}
