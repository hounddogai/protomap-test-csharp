namespace Acme.Workforce.Assets.LicenseSeat.SeatCycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy license seat cycle must pass. Check returns its problems, or none.</summary>
public interface ILegacyLicenseSeatCycleRule
{
    List<string> Check(LegacyLicenseSeatCycle item);
}

public sealed record LegacyLicenseSeatCycleRequiredCodeRule(string Message) : ILegacyLicenseSeatCycleRule
{
    public List<string> Check(LegacyLicenseSeatCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatCycleAllowedDescriptionRule(List<string> Allowed) : ILegacyLicenseSeatCycleRule
{
    public List<string> Check(LegacyLicenseSeatCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is expired" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatCycleRetriesRangeRule(int Min, int Max) : ILegacyLicenseSeatCycleRule
{
    public List<string> Check(LegacyLicenseSeatCycle item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatCycleCodePrefixRule(string Prefix) : ILegacyLicenseSeatCycleRule
{
    public List<string> Check(LegacyLicenseSeatCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatCycleRequiredDescriptionRule(string Message) : ILegacyLicenseSeatCycleRule
{
    public List<string> Check(LegacyLicenseSeatCycle item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy license seat cycle.</summary>
public sealed record LegacyLicenseSeatCycleValidator(List<ILegacyLicenseSeatCycleRule> Rules)
{
    public List<string> Validate(LegacyLicenseSeatCycle item)
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

    public void RequireValid(LegacyLicenseSeatCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyLicenseSeatCycle> items)
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

/// <summary>Functions of legacy license seat cycle rules.</summary>
public static class LegacyLicenseSeatCycleRules
{
    public static LegacyLicenseSeatCycleValidator DefaultLegacyLicenseSeatCycleValidator()
    {
        return new LegacyLicenseSeatCycleValidator(
            new List<ILegacyLicenseSeatCycleRule>
            {
                new LegacyLicenseSeatCycleRequiredCodeRule("legacy license seat cycle needs an entry"),
                new LegacyLicenseSeatCycleAllowedDescriptionRule(new List<string>()),
                new LegacyLicenseSeatCycleRetriesRangeRule(80, 80),
                new LegacyLicenseSeatCycleCodePrefixRule("Q"),
                new LegacyLicenseSeatCycleRequiredDescriptionRule("legacy license seat cycle needs this field"),
            });
    }
}
