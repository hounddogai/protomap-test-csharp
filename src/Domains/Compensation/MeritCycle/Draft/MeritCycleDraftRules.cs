namespace Acme.Workforce.Compensation.MeritCycle.Draft;

/// <summary>A rule a merit cycle draft must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleDraftRule
{
    List<string> Check(MeritCycleDraft item);
}

public sealed record MeritCycleDraftNameLengthRule(int MaxLength) : IMeritCycleDraftRule
{
    public List<string> Check(MeritCycleDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleDraftVisibleGuardRule(int Limit) : IMeritCycleDraftRule
{
    public List<string> Check(MeritCycleDraft item)
    {
        if (item.Visible && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleDraftRequiredTitleRule(string Message) : IMeritCycleDraftRule
{
    public List<string> Check(MeritCycleDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle draft.</summary>
public sealed record MeritCycleDraftValidator(List<IMeritCycleDraftRule> Rules)
{
    public List<string> Validate(MeritCycleDraft item)
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

    public void RequireValid(MeritCycleDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleDraft> items)
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

/// <summary>Functions of merit cycle draft rules.</summary>
public static class MeritCycleDraftRules
{
    public static MeritCycleDraftValidator DefaultMeritCycleDraftValidator()
    {
        return new MeritCycleDraftValidator(
            new List<IMeritCycleDraftRule>
            {
                new MeritCycleDraftNameLengthRule(80),
                new MeritCycleDraftVisibleGuardRule(64),
                new MeritCycleDraftRequiredTitleRule("merit cycle draft needs a value"),
            });
    }
}
