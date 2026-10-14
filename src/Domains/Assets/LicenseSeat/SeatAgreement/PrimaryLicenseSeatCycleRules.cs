namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

/// <summary>A rule a primary license seat cycle must pass. Check returns its problems, or none.</summary>
public interface IPrimaryLicenseSeatCycleRule
{
    List<string> Check(PrimaryLicenseSeatCycle item);
}

public sealed record PrimaryLicenseSeatCycleRequiredDepartmentRule(string Message) : IPrimaryLicenseSeatCycleRule
{
    public List<string> Check(PrimaryLicenseSeatCycle item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLicenseSeatCycleWeightRangeRule(double Min, double Max) : IPrimaryLicenseSeatCycleRule
{
    public List<string> Check(PrimaryLicenseSeatCycle item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryLicenseSeatCycleRequiredNameRule(string Message) : IPrimaryLicenseSeatCycleRule
{
    public List<string> Check(PrimaryLicenseSeatCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary license seat cycle.</summary>
public sealed record PrimaryLicenseSeatCycleValidator(List<IPrimaryLicenseSeatCycleRule> Rules)
{
    public List<string> Validate(PrimaryLicenseSeatCycle item)
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

    public void RequireValid(PrimaryLicenseSeatCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryLicenseSeatCycle> items)
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

/// <summary>Functions of primary license seat cycle rules.</summary>
public static class PrimaryLicenseSeatCycleRules
{
    public static PrimaryLicenseSeatCycleValidator DefaultPrimaryLicenseSeatCycleValidator()
    {
        return new PrimaryLicenseSeatCycleValidator(
            new List<IPrimaryLicenseSeatCycleRule>
            {
                new PrimaryLicenseSeatCycleRequiredDepartmentRule("primary license seat cycle needs an entry"),
                new PrimaryLicenseSeatCycleWeightRangeRule(6.6, 4493.9),
                new PrimaryLicenseSeatCycleRequiredNameRule("primary license seat cycle needs an entry"),
            });
    }
}
