namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient desk profile is in its life cycle.</summary>
public enum LenientDeskProfileStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient desk profile. Its code is unique within a tenant.</summary>
public sealed class LenientDeskProfile(string code, string name, double amount, string notes, double floorValue, string phone, int revision, string skill, string costCenter, int leaveDays, LenientDeskProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string Notes { get; set; } = notes;
    public double FloorValue { get; set; } = floorValue;
    public string Phone { get; set; } = phone;
    public int Revision { get; set; } = revision;
    public string Skill { get; set; } = skill;
    public string CostCenter { get; set; } = costCenter;
    public int LeaveDays { get; set; } = leaveDays;
    public LenientDeskProfileStatus Status { get; set; } = status;

    /// <summary>Starts a lenient desk profile in status created.</summary>
    public static LenientDeskProfile NewLenientDeskProfile(string code, string name)
    {
        return new LenientDeskProfile(code, name, 0.0, "", 0.0, "", 0, "", "", 0, LenientDeskProfileStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientDeskProfileStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientDeskProfileStatus next)
    {
        switch (Status)
        {
            case LenientDeskProfileStatus.Created:
                switch (next)
                {
                    case LenientDeskProfileStatus.Validated:
                    case LenientDeskProfileStatus.Published:
                        return true;
                }
                return false;
            case LenientDeskProfileStatus.Validated:
                return next == LenientDeskProfileStatus.Retired;
            case LenientDeskProfileStatus.Published:
                switch (next)
                {
                    case LenientDeskProfileStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientDeskProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient desk profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"lenient desk profile"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)LeaveDays + FloorValue, 2);
    }

    public LenientDeskProfile Copy()
    {
        return new LenientDeskProfile(
            Code,
            Name,
            Amount,
            Notes,
            FloorValue,
            Phone,
            Revision,
            Skill,
            CostCenter,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
