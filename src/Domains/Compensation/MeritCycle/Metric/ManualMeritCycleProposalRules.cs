namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual merit cycle proposal must pass. Check returns its problems, or none.</summary>
public interface IManualMeritCycleProposalRule
{
    List<string> Check(ManualMeritCycleProposal item);
}

public sealed record ManualMeritCycleProposalHoursRangeRule(double Min, double Max) : IManualMeritCycleProposalRule
{
    public List<string> Check(ManualMeritCycleProposal item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualMeritCycleProposalLabelLengthRule(int MaxLength) : IManualMeritCycleProposalRule
{
    public List<string> Check(ManualMeritCycleProposal item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualMeritCycleProposalFloorValueRangeRule(double Min, double Max) : IManualMeritCycleProposalRule
{
    public List<string> Check(ManualMeritCycleProposal item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualMeritCycleProposalCodePrefixRule(string Prefix) : IManualMeritCycleProposalRule
{
    public List<string> Check(ManualMeritCycleProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualMeritCycleProposalAllowedCodeRule(List<string> Allowed) : IManualMeritCycleProposalRule
{
    public List<string> Check(ManualMeritCycleProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record ManualMeritCycleProposalRequiredCountryRule(string Message) : IManualMeritCycleProposalRule
{
    public List<string> Check(ManualMeritCycleProposal item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual merit cycle proposal.</summary>
public sealed record ManualMeritCycleProposalValidator(List<IManualMeritCycleProposalRule> Rules)
{
    public List<string> Validate(ManualMeritCycleProposal item)
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

    public void RequireValid(ManualMeritCycleProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualMeritCycleProposal> items)
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

/// <summary>Functions of manual merit cycle proposal rules.</summary>
public static class ManualMeritCycleProposalRules
{
    public static ManualMeritCycleProposalValidator DefaultManualMeritCycleProposalValidator()
    {
        return new ManualMeritCycleProposalValidator(
            new List<IManualMeritCycleProposalRule>
            {
                new ManualMeritCycleProposalHoursRangeRule(3.1, 4818.9),
                new ManualMeritCycleProposalLabelLengthRule(32),
                new ManualMeritCycleProposalFloorValueRangeRule(9.4, 1311.2),
                new ManualMeritCycleProposalCodePrefixRule("Q"),
                new ManualMeritCycleProposalAllowedCodeRule(new List<string>()),
                new ManualMeritCycleProposalRequiredCountryRule("manual merit cycle proposal needs this field"),
            });
    }
}
