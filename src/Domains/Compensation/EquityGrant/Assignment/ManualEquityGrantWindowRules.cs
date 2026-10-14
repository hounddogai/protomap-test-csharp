namespace Acme.Workforce.Compensation.EquityGrant.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual equity grant window must pass. Check returns its problems, or none.</summary>
public interface IManualEquityGrantWindowRule
{
    List<string> Check(ManualEquityGrantWindow item);
}

public sealed record ManualEquityGrantWindowPriorityRangeRule(int Min, int Max) : IManualEquityGrantWindowRule
{
    public List<string> Check(ManualEquityGrantWindow item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualEquityGrantWindowAllowedCodeRule(List<string> Allowed) : IManualEquityGrantWindowRule
{
    public List<string> Check(ManualEquityGrantWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record ManualEquityGrantWindowActiveGuardRule(int Limit) : IManualEquityGrantWindowRule
{
    public List<string> Check(ManualEquityGrantWindow item)
    {
        if (item.Active && item.DurationMinutes > Limit)
        {
            return new List<string> { $"duration minutes {item.DurationMinutes} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record ManualEquityGrantWindowBadgeLengthRule(int MaxLength) : IManualEquityGrantWindowRule
{
    public List<string> Check(ManualEquityGrantWindow item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualEquityGrantWindowRequiredPhoneRule(string Message) : IManualEquityGrantWindowRule
{
    public List<string> Check(ManualEquityGrantWindow item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual equity grant window.</summary>
public sealed record ManualEquityGrantWindowValidator(List<IManualEquityGrantWindowRule> Rules)
{
    public List<string> Validate(ManualEquityGrantWindow item)
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

    public void RequireValid(ManualEquityGrantWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualEquityGrantWindow> items)
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

/// <summary>Functions of manual equity grant window rules.</summary>
public static class ManualEquityGrantWindowRules
{
    public static ManualEquityGrantWindowValidator DefaultManualEquityGrantWindowValidator()
    {
        return new ManualEquityGrantWindowValidator(
            new List<IManualEquityGrantWindowRule>
            {
                new ManualEquityGrantWindowPriorityRangeRule(16, 86400),
                new ManualEquityGrantWindowAllowedCodeRule(new List<string> { "manual", "tiered" }),
                new ManualEquityGrantWindowActiveGuardRule(1000),
                new ManualEquityGrantWindowBadgeLengthRule(64),
                new ManualEquityGrantWindowRequiredPhoneRule("manual equity grant window needs a value"),
            });
    }
}
