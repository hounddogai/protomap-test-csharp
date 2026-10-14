namespace Acme.Workforce.Payrollops.RetroPay.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a retro pay schedule must pass. Check returns its problems, or none.</summary>
public interface IRetroPayScheduleRule
{
    List<string> Check(RetroPaySchedule item);
}

public sealed record RetroPayScheduleOwnerIdLengthRule(int MaxLength) : IRetroPayScheduleRule
{
    public List<string> Check(RetroPaySchedule item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayScheduleAllowedLabelRule(List<string> Allowed) : IRetroPayScheduleRule
{
    public List<string> Check(RetroPaySchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayScheduleRequiredCodeRule(string Message) : IRetroPayScheduleRule
{
    public List<string> Check(RetroPaySchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayScheduleLabelLengthRule(int MaxLength) : IRetroPayScheduleRule
{
    public List<string> Check(RetroPaySchedule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayScheduleVisibleGuardRule(int Limit) : IRetroPayScheduleRule
{
    public List<string> Check(RetroPaySchedule item)
    {
        if (item.Visible && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayScheduleRequiredLabelRule(string Message) : IRetroPayScheduleRule
{
    public List<string> Check(RetroPaySchedule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay schedule.</summary>
public sealed record RetroPayScheduleValidator(List<IRetroPayScheduleRule> Rules)
{
    public List<string> Validate(RetroPaySchedule item)
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

    public void RequireValid(RetroPaySchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPaySchedule> items)
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

/// <summary>Functions of retro pay schedule rules.</summary>
public static class RetroPayScheduleRules
{
    public static RetroPayScheduleValidator DefaultRetroPayScheduleValidator()
    {
        return new RetroPayScheduleValidator(
            new List<IRetroPayScheduleRule>
            {
                new RetroPayScheduleOwnerIdLengthRule(32),
                new RetroPayScheduleAllowedLabelRule(new List<string> { "rolling", "seasonal", "default" }),
                new RetroPayScheduleRequiredCodeRule("retro pay schedule needs an entry"),
                new RetroPayScheduleLabelLengthRule(80),
                new RetroPayScheduleVisibleGuardRule(120),
                new RetroPayScheduleRequiredLabelRule("retro pay schedule needs this field"),
            });
    }
}
