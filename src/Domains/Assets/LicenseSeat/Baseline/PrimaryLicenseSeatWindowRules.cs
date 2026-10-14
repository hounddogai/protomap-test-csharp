namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary license seat window must pass. Check returns its problems, or none.</summary>
public interface IPrimaryLicenseSeatWindowRule
{
    List<string> Check(PrimaryLicenseSeatWindow item);
}

public sealed record PrimaryLicenseSeatWindowRequiredLabelRule(string Message) : IPrimaryLicenseSeatWindowRule
{
    public List<string> Check(PrimaryLicenseSeatWindow item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLicenseSeatWindowRequiredCostCenterRule(string Message) : IPrimaryLicenseSeatWindowRule
{
    public List<string> Check(PrimaryLicenseSeatWindow item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLicenseSeatWindowAllowedEmployeeIdRule(List<string> Allowed) : IPrimaryLicenseSeatWindowRule
{
    public List<string> Check(PrimaryLicenseSeatWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary license seat window.</summary>
public sealed record PrimaryLicenseSeatWindowValidator(List<IPrimaryLicenseSeatWindowRule> Rules)
{
    public List<string> Validate(PrimaryLicenseSeatWindow item)
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

    public void RequireValid(PrimaryLicenseSeatWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryLicenseSeatWindow> items)
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

/// <summary>Functions of primary license seat window rules.</summary>
public static class PrimaryLicenseSeatWindowRules
{
    public static PrimaryLicenseSeatWindowValidator DefaultPrimaryLicenseSeatWindowValidator()
    {
        return new PrimaryLicenseSeatWindowValidator(
            new List<IPrimaryLicenseSeatWindowRule>
            {
                new PrimaryLicenseSeatWindowRequiredLabelRule("primary license seat window needs an entry"),
                new PrimaryLicenseSeatWindowRequiredCostCenterRule("primary license seat window needs this field"),
                new PrimaryLicenseSeatWindowAllowedEmployeeIdRule(new List<string> { "primary", "batched" }),
            });
    }
}
