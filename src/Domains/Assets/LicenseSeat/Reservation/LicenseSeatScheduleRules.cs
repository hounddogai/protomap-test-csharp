namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>A rule a license seat schedule must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatScheduleRule
{
    List<string> Check(LicenseSeatSchedule item);
}

public sealed record LicenseSeatScheduleSalaryBandLengthRule(int MaxLength) : ILicenseSeatScheduleRule
{
    public List<string> Check(LicenseSeatSchedule item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatScheduleNameLengthRule(int MaxLength) : ILicenseSeatScheduleRule
{
    public List<string> Check(LicenseSeatSchedule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatScheduleOwnerIdLengthRule(int MaxLength) : ILicenseSeatScheduleRule
{
    public List<string> Check(LicenseSeatSchedule item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat schedule.</summary>
public sealed record LicenseSeatScheduleValidator(List<ILicenseSeatScheduleRule> Rules)
{
    public List<string> Validate(LicenseSeatSchedule item)
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

    public void RequireValid(LicenseSeatSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatSchedule> items)
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

/// <summary>Functions of license seat schedule rules.</summary>
public static class LicenseSeatScheduleRules
{
    public static LicenseSeatScheduleValidator DefaultLicenseSeatScheduleValidator()
    {
        return new LicenseSeatScheduleValidator(
            new List<ILicenseSeatScheduleRule>
            {
                new LicenseSeatScheduleSalaryBandLengthRule(32),
                new LicenseSeatScheduleNameLengthRule(64),
                new LicenseSeatScheduleOwnerIdLengthRule(86400),
            });
    }
}
