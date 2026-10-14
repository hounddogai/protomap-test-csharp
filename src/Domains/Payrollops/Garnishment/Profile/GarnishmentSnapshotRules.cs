namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment snapshot must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentSnapshotRule
{
    List<string> Check(GarnishmentSnapshot item);
}

public sealed record GarnishmentSnapshotAllowedCodeRule(List<string> Allowed) : IGarnishmentSnapshotRule
{
    public List<string> Check(GarnishmentSnapshot item)
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

public sealed record GarnishmentSnapshotCodePrefixRule(string Prefix) : IGarnishmentSnapshotRule
{
    public List<string> Check(GarnishmentSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentSnapshotRequiredCodeRule(string Message) : IGarnishmentSnapshotRule
{
    public List<string> Check(GarnishmentSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentSnapshotEndsAtRangeRule(int Min, int Max) : IGarnishmentSnapshotRule
{
    public List<string> Check(GarnishmentSnapshot item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a garnishment snapshot.</summary>
public sealed record GarnishmentSnapshotValidator(List<IGarnishmentSnapshotRule> Rules)
{
    public List<string> Validate(GarnishmentSnapshot item)
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

    public void RequireValid(GarnishmentSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentSnapshot> items)
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

/// <summary>Functions of garnishment snapshot rules.</summary>
public static class GarnishmentSnapshotRules
{
    public static GarnishmentSnapshotValidator DefaultGarnishmentSnapshotValidator()
    {
        return new GarnishmentSnapshotValidator(
            new List<IGarnishmentSnapshotRule>
            {
                new GarnishmentSnapshotAllowedCodeRule(new List<string>()),
                new GarnishmentSnapshotCodePrefixRule("ACM"),
                new GarnishmentSnapshotRequiredCodeRule("garnishment snapshot needs an entry"),
                new GarnishmentSnapshotEndsAtRangeRule(64, 1000),
            });
    }
}
