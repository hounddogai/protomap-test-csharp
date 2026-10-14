namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop audit must pass. Check returns its problems, or none.</summary>
public interface ILaptopAuditRule
{
    List<string> Check(LaptopAudit item);
}

public sealed record LaptopAuditStartsAtRangeRule(int Min, int Max) : ILaptopAuditRule
{
    public List<string> Check(LaptopAudit item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopAuditCodeLengthRule(int MaxLength) : ILaptopAuditRule
{
    public List<string> Check(LaptopAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopAuditAllowedChecksumRule(List<string> Allowed) : ILaptopAuditRule
{
    public List<string> Check(LaptopAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is missing" };
        }
        return new List<string>();
    }
}

public sealed record LaptopAuditRequiredChecksumRule(string Message) : ILaptopAuditRule
{
    public List<string> Check(LaptopAudit item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop audit.</summary>
public sealed record LaptopAuditValidator(List<ILaptopAuditRule> Rules)
{
    public List<string> Validate(LaptopAudit item)
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

    public void RequireValid(LaptopAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopAudit> items)
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

/// <summary>Functions of laptop audit rules.</summary>
public static class LaptopAuditRules
{
    public static LaptopAuditValidator DefaultLaptopAuditValidator()
    {
        return new LaptopAuditValidator(
            new List<ILaptopAuditRule>
            {
                new LaptopAuditStartsAtRangeRule(255, 86400),
                new LaptopAuditCodeLengthRule(80),
                new LaptopAuditAllowedChecksumRule(new List<string> { "default", "legacy", "weighted" }),
                new LaptopAuditRequiredChecksumRule("laptop audit needs a value"),
            });
    }
}
