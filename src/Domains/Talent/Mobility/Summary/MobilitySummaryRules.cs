namespace Acme.Workforce.Talent.Mobility.Summary;

/// <summary>A rule a mobility summary must pass. Check returns its problems, or none.</summary>
public interface IMobilitySummaryRule
{
    List<string> Check(MobilitySummary item);
}

public sealed record MobilitySummaryTenureMonthsRangeRule(int Min, int Max) : IMobilitySummaryRule
{
    public List<string> Check(MobilitySummary item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilitySummaryCodeLengthRule(int MaxLength) : IMobilitySummaryRule
{
    public List<string> Check(MobilitySummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilitySummaryRequiredCodeRule(string Message) : IMobilitySummaryRule
{
    public List<string> Check(MobilitySummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilitySummaryPinnedGuardRule(double Limit) : IMobilitySummaryRule
{
    public List<string> Check(MobilitySummary item)
    {
        if (item.Pinned && item.Budget > Limit)
        {
            return new List<string> { $"budget {item.Budget} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility summary.</summary>
public sealed record MobilitySummaryValidator(List<IMobilitySummaryRule> Rules)
{
    public List<string> Validate(MobilitySummary item)
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

    public void RequireValid(MobilitySummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilitySummary> items)
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

/// <summary>Functions of mobility summary rules.</summary>
public static class MobilitySummaryRules
{
    public static MobilitySummaryValidator DefaultMobilitySummaryValidator()
    {
        return new MobilitySummaryValidator(
            new List<IMobilitySummaryRule>
            {
                new MobilitySummaryTenureMonthsRangeRule(32, 255),
                new MobilitySummaryCodeLengthRule(80),
                new MobilitySummaryRequiredCodeRule("mobility summary needs an entry"),
                new MobilitySummaryPinnedGuardRule(7.7),
            });
    }
}
