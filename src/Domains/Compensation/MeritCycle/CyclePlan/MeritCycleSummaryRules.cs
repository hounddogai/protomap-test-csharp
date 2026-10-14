namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>A rule a merit cycle summary must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleSummaryRule
{
    List<string> Check(MeritCycleSummary item);
}

public sealed record MeritCycleSummaryRequiredCodeRule(string Message) : IMeritCycleSummaryRule
{
    public List<string> Check(MeritCycleSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleSummaryLabelLengthRule(int MaxLength) : IMeritCycleSummaryRule
{
    public List<string> Check(MeritCycleSummary item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleSummaryNotesLengthRule(int MaxLength) : IMeritCycleSummaryRule
{
    public List<string> Check(MeritCycleSummary item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleSummaryBadgeLengthRule(int MaxLength) : IMeritCycleSummaryRule
{
    public List<string> Check(MeritCycleSummary item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleSummaryCodeLengthRule(int MaxLength) : IMeritCycleSummaryRule
{
    public List<string> Check(MeritCycleSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleSummaryWeightRangeRule(double Min, double Max) : IMeritCycleSummaryRule
{
    public List<string> Check(MeritCycleSummary item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle summary.</summary>
public sealed record MeritCycleSummaryValidator(List<IMeritCycleSummaryRule> Rules)
{
    public List<string> Validate(MeritCycleSummary item)
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

    public void RequireValid(MeritCycleSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleSummary> items)
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

/// <summary>Functions of merit cycle summary rules.</summary>
public static class MeritCycleSummaryRules
{
    public static MeritCycleSummaryValidator DefaultMeritCycleSummaryValidator()
    {
        return new MeritCycleSummaryValidator(
            new List<IMeritCycleSummaryRule>
            {
                new MeritCycleSummaryRequiredCodeRule("merit cycle summary needs an entry"),
                new MeritCycleSummaryLabelLengthRule(1000),
                new MeritCycleSummaryNotesLengthRule(1000),
                new MeritCycleSummaryBadgeLengthRule(3600),
                new MeritCycleSummaryCodeLengthRule(120),
                new MeritCycleSummaryWeightRangeRule(0.3, 2330.0),
            });
    }
}
