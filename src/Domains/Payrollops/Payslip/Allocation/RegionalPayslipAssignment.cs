namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional payslip assignment is in its life cycle.</summary>
public enum RegionalPayslipAssignmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A regional payslip assignment as the payslip team keeps it.</summary>
public sealed class RegionalPayslipAssignment(string code, string name, int startsAt, string locale, string phone, string location, int sequence, string skill, List<string> tags, double hours, RegionalPayslipAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public string Locale { get; set; } = locale;
    public string Phone { get; set; } = phone;
    public string Location { get; set; } = location;
    public int Sequence { get; set; } = sequence;
    public string Skill { get; set; } = skill;
    public List<string> Tags { get; set; } = tags;
    public double Hours { get; set; } = hours;
    public RegionalPayslipAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a regional payslip assignment in status requested.</summary>
    public static RegionalPayslipAssignment NewRegionalPayslipAssignment(string code, string name)
    {
        return new RegionalPayslipAssignment(
            code,
            name,
            0,
            "",
            "",
            "",
            0,
            "",
            new List<string>(),
            0.0,
            RegionalPayslipAssignmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPayslipAssignmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPayslipAssignmentStatus next)
    {
        switch (Status)
        {
            case RegionalPayslipAssignmentStatus.Requested:
                return next == RegionalPayslipAssignmentStatus.Expired;
            case RegionalPayslipAssignmentStatus.Granted:
                switch (next)
                {
                    case RegionalPayslipAssignmentStatus.Expired:
                        return true;
                }
                return false;
            case RegionalPayslipAssignmentStatus.Revoked:
                switch (next)
                {
                    case RegionalPayslipAssignmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalPayslipAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional payslip assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedLoad()
    {
        return Math.Round(Hours + (double)StartsAt, 2);
    }

    public RegionalPayslipAssignment Copy()
    {
        return new RegionalPayslipAssignment(
            Code,
            Name,
            StartsAt,
            Locale,
            Phone,
            Location,
            Sequence,
            Skill,
            Tags,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
