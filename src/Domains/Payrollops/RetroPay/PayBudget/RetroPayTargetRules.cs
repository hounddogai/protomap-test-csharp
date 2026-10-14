namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a retro pay target must pass. Check returns its problems, or none.</summary>
public interface IRetroPayTargetRule
{
    List<string> Check(RetroPayTarget item);
}

public sealed record RetroPayTargetTagsLimitRule(int MaxCount, List<string> Banned) : IRetroPayTargetRule
{
    public List<string> Check(RetroPayTarget item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record RetroPayTargetTeamLengthRule(int MaxLength) : IRetroPayTargetRule
{
    public List<string> Check(RetroPayTarget item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayTargetRequiredManagerIdRule(string Message) : IRetroPayTargetRule
{
    public List<string> Check(RetroPayTarget item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayTargetRequiredCodeRule(string Message) : IRetroPayTargetRule
{
    public List<string> Check(RetroPayTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayTargetAllowedCodeRule(List<string> Allowed) : IRetroPayTargetRule
{
    public List<string> Check(RetroPayTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay target.</summary>
public sealed record RetroPayTargetValidator(List<IRetroPayTargetRule> Rules)
{
    public List<string> Validate(RetroPayTarget item)
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

    public void RequireValid(RetroPayTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayTarget> items)
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

/// <summary>Functions of retro pay target rules.</summary>
public static class RetroPayTargetRules
{
    public static RetroPayTargetValidator DefaultRetroPayTargetValidator()
    {
        return new RetroPayTargetValidator(
            new List<IRetroPayTargetRule>
            {
                new RetroPayTargetTagsLimitRule(1000, new List<string> { "regional", "regional" }),
                new RetroPayTargetTeamLengthRule(3600),
                new RetroPayTargetRequiredManagerIdRule("retro pay target needs a value"),
                new RetroPayTargetRequiredCodeRule("retro pay target needs a value"),
                new RetroPayTargetAllowedCodeRule(new List<string>()),
            });
    }
}
