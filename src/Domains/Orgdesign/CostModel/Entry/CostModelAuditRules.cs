namespace Acme.Workforce.Orgdesign.CostModel.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a cost model audit must pass. Check returns its problems, or none.</summary>
public interface ICostModelAuditRule
{
    List<string> Check(CostModelAudit item);
}

public sealed record CostModelAuditCreatedAtRangeRule(int Min, int Max) : ICostModelAuditRule
{
    public List<string> Check(CostModelAudit item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelAuditRequiredSourceRule(string Message) : ICostModelAuditRule
{
    public List<string> Check(CostModelAudit item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelAuditTitleLengthRule(int MaxLength) : ICostModelAuditRule
{
    public List<string> Check(CostModelAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelAuditAllowedLocationRule(List<string> Allowed) : ICostModelAuditRule
{
    public List<string> Check(CostModelAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record CostModelAuditLocationLengthRule(int MaxLength) : ICostModelAuditRule
{
    public List<string> Check(CostModelAudit item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelAuditSourceLengthRule(int MaxLength) : ICostModelAuditRule
{
    public List<string> Check(CostModelAudit item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model audit.</summary>
public sealed record CostModelAuditValidator(List<ICostModelAuditRule> Rules)
{
    public List<string> Validate(CostModelAudit item)
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

    public void RequireValid(CostModelAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelAudit> items)
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

/// <summary>Functions of cost model audit rules.</summary>
public static class CostModelAuditRules
{
    public static CostModelAuditValidator DefaultCostModelAuditValidator()
    {
        return new CostModelAuditValidator(
            new List<ICostModelAuditRule>
            {
                new CostModelAuditCreatedAtRangeRule(32, 86400),
                new CostModelAuditRequiredSourceRule("cost model audit needs an entry"),
                new CostModelAuditTitleLengthRule(80),
                new CostModelAuditAllowedLocationRule(new List<string>()),
                new CostModelAuditLocationLengthRule(16),
                new CostModelAuditSourceLengthRule(32),
            });
    }
}
