namespace Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>A rule a merit cycle proposal must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleProposalRule
{
    List<string> Check(MeritCycleProposal item);
}

public sealed record MeritCycleProposalNameLengthRule(int MaxLength) : IMeritCycleProposalRule
{
    public List<string> Check(MeritCycleProposal item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleProposalTenureMonthsRangeRule(int Min, int Max) : IMeritCycleProposalRule
{
    public List<string> Check(MeritCycleProposal item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleProposalEmployeeIdLengthRule(int MaxLength) : IMeritCycleProposalRule
{
    public List<string> Check(MeritCycleProposal item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleProposalRequiredCodeRule(string Message) : IMeritCycleProposalRule
{
    public List<string> Check(MeritCycleProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle proposal.</summary>
public sealed record MeritCycleProposalValidator(List<IMeritCycleProposalRule> Rules)
{
    public List<string> Validate(MeritCycleProposal item)
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

    public void RequireValid(MeritCycleProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleProposal> items)
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

/// <summary>Functions of merit cycle proposal rules.</summary>
public static class MeritCycleProposalRules
{
    public static MeritCycleProposalValidator DefaultMeritCycleProposalValidator()
    {
        return new MeritCycleProposalValidator(
            new List<IMeritCycleProposalRule>
            {
                new MeritCycleProposalNameLengthRule(255),
                new MeritCycleProposalTenureMonthsRangeRule(16, 80),
                new MeritCycleProposalEmployeeIdLengthRule(86400),
                new MeritCycleProposalRequiredCodeRule("merit cycle proposal needs a value"),
            });
    }
}
