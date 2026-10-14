namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment cycle must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentCycleRule
{
    List<string> Check(GarnishmentCycle item);
}

public sealed record GarnishmentCycleCodeLengthRule(int MaxLength) : IGarnishmentCycleRule
{
    public List<string> Check(GarnishmentCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentCyclePriorityRangeRule(int Min, int Max) : IGarnishmentCycleRule
{
    public List<string> Check(GarnishmentCycle item)
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

public sealed record GarnishmentCycleRequiredUnitRule(string Message) : IGarnishmentCycleRule
{
    public List<string> Check(GarnishmentCycle item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentCycleRequiredNameRule(string Message) : IGarnishmentCycleRule
{
    public List<string> Check(GarnishmentCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentCycleNameLengthRule(int MaxLength) : IGarnishmentCycleRule
{
    public List<string> Check(GarnishmentCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentCycleAllowedJobCodeRule(List<string> Allowed) : IGarnishmentCycleRule
{
    public List<string> Check(GarnishmentCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment cycle.</summary>
public sealed record GarnishmentCycleValidator(List<IGarnishmentCycleRule> Rules)
{
    public List<string> Validate(GarnishmentCycle item)
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

    public void RequireValid(GarnishmentCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentCycle> items)
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

/// <summary>Functions of garnishment cycle rules.</summary>
public static class GarnishmentCycleRules
{
    public static GarnishmentCycleValidator DefaultGarnishmentCycleValidator()
    {
        return new GarnishmentCycleValidator(
            new List<IGarnishmentCycleRule>
            {
                new GarnishmentCycleCodeLengthRule(255),
                new GarnishmentCyclePriorityRangeRule(80, 86400),
                new GarnishmentCycleRequiredUnitRule("garnishment cycle needs this field"),
                new GarnishmentCycleRequiredNameRule("garnishment cycle needs a value"),
                new GarnishmentCycleNameLengthRule(86400),
                new GarnishmentCycleAllowedJobCodeRule(new List<string>()),
            });
    }
}
