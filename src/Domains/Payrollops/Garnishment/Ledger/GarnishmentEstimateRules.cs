namespace Acme.Workforce.Payrollops.Garnishment.Ledger;

/// <summary>A rule a garnishment estimate must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentEstimateRule
{
    List<string> Check(GarnishmentEstimate item);
}

public sealed record GarnishmentEstimateRequiredCodeRule(string Message) : IGarnishmentEstimateRule
{
    public List<string> Check(GarnishmentEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentEstimateRequiredNotesRule(string Message) : IGarnishmentEstimateRule
{
    public List<string> Check(GarnishmentEstimate item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentEstimateNameLengthRule(int MaxLength) : IGarnishmentEstimateRule
{
    public List<string> Check(GarnishmentEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment estimate.</summary>
public sealed record GarnishmentEstimateValidator(List<IGarnishmentEstimateRule> Rules)
{
    public List<string> Validate(GarnishmentEstimate item)
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

    public void RequireValid(GarnishmentEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentEstimate> items)
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

/// <summary>Functions of garnishment estimate rules.</summary>
public static class GarnishmentEstimateRules
{
    public static GarnishmentEstimateValidator DefaultGarnishmentEstimateValidator()
    {
        return new GarnishmentEstimateValidator(
            new List<IGarnishmentEstimateRule>
            {
                new GarnishmentEstimateRequiredCodeRule("garnishment estimate needs an entry"),
                new GarnishmentEstimateRequiredNotesRule("garnishment estimate needs an entry"),
                new GarnishmentEstimateNameLengthRule(32),
            });
    }
}
