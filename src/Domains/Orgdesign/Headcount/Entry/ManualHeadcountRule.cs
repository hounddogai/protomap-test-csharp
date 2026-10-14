namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual headcount rule is in its life cycle.</summary>
public enum ManualHeadcountRuleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A manual headcount rule as the headcount team keeps it.</summary>
public sealed class ManualHeadcountRule(string code, string name, int leaveDays, int limit, double budget, double minimum, string phone, string ownerId, string salaryBand, string location, ManualHeadcountRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public int Limit { get; set; } = limit;
    public double Budget { get; set; } = budget;
    public double Minimum { get; set; } = minimum;
    public string Phone { get; set; } = phone;
    public string OwnerId { get; set; } = ownerId;
    public string SalaryBand { get; set; } = salaryBand;
    public string Location { get; set; } = location;
    public ManualHeadcountRuleStatus Status { get; set; } = status;

    /// <summary>Starts a manual headcount rule in status requested.</summary>
    public static ManualHeadcountRule NewManualHeadcountRule(string code, string name)
    {
        return new ManualHeadcountRule(
            code,
            name,
            0,
            0,
            0.0,
            0.0,
            "",
            "",
            "",
            "",
            ManualHeadcountRuleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualHeadcountRuleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualHeadcountRuleStatus next)
    {
        switch (Status)
        {
            case ManualHeadcountRuleStatus.Requested:
                switch (next)
                {
                    case ManualHeadcountRuleStatus.Granted:
                        return true;
                }
                return false;
            case ManualHeadcountRuleStatus.Granted:
                return next == ManualHeadcountRuleStatus.Revoked || next == ManualHeadcountRuleStatus.Expired;
            case ManualHeadcountRuleStatus.Revoked:
                switch (next)
                {
                    case ManualHeadcountRuleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualHeadcountRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual headcount rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Minimum + (double)Limit, 2);
    }

    public ManualHeadcountRule Copy()
    {
        return new ManualHeadcountRule(
            Code,
            Name,
            LeaveDays,
            Limit,
            Budget,
            Minimum,
            Phone,
            OwnerId,
            SalaryBand,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
