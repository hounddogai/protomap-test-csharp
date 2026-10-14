namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a retro pay entry must pass. Check returns its problems, or none.</summary>
public interface IRetroPayEntryRule
{
    List<string> Check(RetroPayEntry item);
}

public sealed record RetroPayEntryLocationLengthRule(int MaxLength) : IRetroPayEntryRule
{
    public List<string> Check(RetroPayEntry item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayEntryNameLengthRule(int MaxLength) : IRetroPayEntryRule
{
    public List<string> Check(RetroPayEntry item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayEntryAllowedCodeRule(List<string> Allowed) : IRetroPayEntryRule
{
    public List<string> Check(RetroPayEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayEntryRequiredLabelRule(string Message) : IRetroPayEntryRule
{
    public List<string> Check(RetroPayEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayEntryLabelLengthRule(int MaxLength) : IRetroPayEntryRule
{
    public List<string> Check(RetroPayEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay entry.</summary>
public sealed record RetroPayEntryValidator(List<IRetroPayEntryRule> Rules)
{
    public List<string> Validate(RetroPayEntry item)
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

    public void RequireValid(RetroPayEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayEntry> items)
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

/// <summary>Functions of retro pay entry rules.</summary>
public static class RetroPayEntryRules
{
    public static RetroPayEntryValidator DefaultRetroPayEntryValidator()
    {
        return new RetroPayEntryValidator(
            new List<IRetroPayEntryRule>
            {
                new RetroPayEntryLocationLengthRule(64),
                new RetroPayEntryNameLengthRule(64),
                new RetroPayEntryAllowedCodeRule(new List<string> { "batched", "legacy" }),
                new RetroPayEntryRequiredLabelRule("retro pay entry needs this field"),
                new RetroPayEntryLabelLengthRule(120),
            });
    }
}
