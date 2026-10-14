namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment score must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentScoreRule
{
    List<string> Check(GarnishmentScore item);
}

public sealed record GarnishmentScoreArchivedGuardRule(int Limit) : IGarnishmentScoreRule
{
    public List<string> Check(GarnishmentScore item)
    {
        if (item.Archived && item.DurationMinutes > Limit)
        {
            return new List<string> { $"duration minutes {item.DurationMinutes} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentScoreAllowedNameRule(List<string> Allowed) : IGarnishmentScoreRule
{
    public List<string> Check(GarnishmentScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentScoreEmployeeIdLengthRule(int MaxLength) : IGarnishmentScoreRule
{
    public List<string> Check(GarnishmentScore item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentScoreCodeLengthRule(int MaxLength) : IGarnishmentScoreRule
{
    public List<string> Check(GarnishmentScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment score.</summary>
public sealed record GarnishmentScoreValidator(List<IGarnishmentScoreRule> Rules)
{
    public List<string> Validate(GarnishmentScore item)
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

    public void RequireValid(GarnishmentScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentScore> items)
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

/// <summary>Functions of garnishment score rules.</summary>
public static class GarnishmentScoreRules
{
    public static GarnishmentScoreValidator DefaultGarnishmentScoreValidator()
    {
        return new GarnishmentScoreValidator(
            new List<IGarnishmentScoreRule>
            {
                new GarnishmentScoreArchivedGuardRule(64),
                new GarnishmentScoreAllowedNameRule(new List<string> { "pending" }),
                new GarnishmentScoreEmployeeIdLengthRule(86400),
                new GarnishmentScoreCodeLengthRule(64),
            });
    }
}
