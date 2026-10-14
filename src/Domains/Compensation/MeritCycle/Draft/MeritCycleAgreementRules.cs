namespace Acme.Workforce.Compensation.MeritCycle.Draft;

/// <summary>A rule a merit cycle agreement must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleAgreementRule
{
    List<string> Check(MeritCycleAgreement item);
}

public sealed record MeritCycleAgreementCodeLengthRule(int MaxLength) : IMeritCycleAgreementRule
{
    public List<string> Check(MeritCycleAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleAgreementFloorValueRangeRule(double Min, double Max) : IMeritCycleAgreementRule
{
    public List<string> Check(MeritCycleAgreement item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleAgreementRequiredNameRule(string Message) : IMeritCycleAgreementRule
{
    public List<string> Check(MeritCycleAgreement item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleAgreementDescriptionLengthRule(int MaxLength) : IMeritCycleAgreementRule
{
    public List<string> Check(MeritCycleAgreement item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleAgreementJobCodeLengthRule(int MaxLength) : IMeritCycleAgreementRule
{
    public List<string> Check(MeritCycleAgreement item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle agreement.</summary>
public sealed record MeritCycleAgreementValidator(List<IMeritCycleAgreementRule> Rules)
{
    public List<string> Validate(MeritCycleAgreement item)
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

    public void RequireValid(MeritCycleAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleAgreement> items)
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

/// <summary>Functions of merit cycle agreement rules.</summary>
public static class MeritCycleAgreementRules
{
    public static MeritCycleAgreementValidator DefaultMeritCycleAgreementValidator()
    {
        return new MeritCycleAgreementValidator(
            new List<IMeritCycleAgreementRule>
            {
                new MeritCycleAgreementCodeLengthRule(64),
                new MeritCycleAgreementFloorValueRangeRule(5.0, 703.4),
                new MeritCycleAgreementRequiredNameRule("merit cycle agreement needs this field"),
                new MeritCycleAgreementDescriptionLengthRule(32),
                new MeritCycleAgreementJobCodeLengthRule(80),
            });
    }
}
