namespace Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>A rule a visitor preference must pass. Check returns its problems, or none.</summary>
public interface IVisitorPreferenceRule
{
    List<string> Check(VisitorPreference item);
}

public sealed record VisitorPreferenceRequiredPhoneRule(string Message) : IVisitorPreferenceRule
{
    public List<string> Check(VisitorPreference item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorPreferencePinnedGuardRule(int Limit) : IVisitorPreferenceRule
{
    public List<string> Check(VisitorPreference item)
    {
        if (item.Pinned && item.Priority > Limit)
        {
            return new List<string> { $"priority {item.Priority} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record VisitorPreferenceRequiredCodeRule(string Message) : IVisitorPreferenceRule
{
    public List<string> Check(VisitorPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor preference.</summary>
public sealed record VisitorPreferenceValidator(List<IVisitorPreferenceRule> Rules)
{
    public List<string> Validate(VisitorPreference item)
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

    public void RequireValid(VisitorPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorPreference> items)
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

/// <summary>Functions of visitor preference rules.</summary>
public static class VisitorPreferenceRules
{
    public static VisitorPreferenceValidator DefaultVisitorPreferenceValidator()
    {
        return new VisitorPreferenceValidator(
            new List<IVisitorPreferenceRule>
            {
                new VisitorPreferenceRequiredPhoneRule("visitor preference needs a value"),
                new VisitorPreferencePinnedGuardRule(1000),
                new VisitorPreferenceRequiredCodeRule("visitor preference needs an entry"),
            });
    }
}
