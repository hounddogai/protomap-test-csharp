namespace Acme.Workforce.Talent.Nomination.NominationRollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default nomination rollup is in its life cycle.</summary>
public enum DefaultNominationRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default nomination rollup. Its code is unique within a tenant.</summary>
public sealed class DefaultNominationRollup(string code, string label, string description, string contactEmail, double floorValue, string notes, string phone, string salaryBand, string channel, DefaultNominationRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public string ContactEmail { get; set; } = contactEmail;
    public double FloorValue { get; set; } = floorValue;
    public string Notes { get; set; } = notes;
    public string Phone { get; set; } = phone;
    public string SalaryBand { get; set; } = salaryBand;
    public string Channel { get; set; } = channel;
    public DefaultNominationRollupStatus Status { get; set; } = status;

    /// <summary>Starts a default nomination rollup in status proposed.</summary>
    public static DefaultNominationRollup NewDefaultNominationRollup(string code, string label)
    {
        return new DefaultNominationRollup(
            code,
            label,
            "",
            "",
            0.0,
            "",
            "",
            "",
            "",
            DefaultNominationRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultNominationRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultNominationRollupStatus next)
    {
        switch (Status)
        {
            case DefaultNominationRollupStatus.Proposed:
                switch (next)
                {
                    case DefaultNominationRollupStatus.Rejected:
                    case DefaultNominationRollupStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultNominationRollupStatus.Approved:
                return next == DefaultNominationRollupStatus.Rejected;
            case DefaultNominationRollupStatus.Rejected:
                return next == DefaultNominationRollupStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DefaultNominationRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default nomination rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public DefaultNominationRollup Copy()
    {
        return new DefaultNominationRollup(
            Code,
            Label,
            Description,
            ContactEmail,
            FloorValue,
            Notes,
            Phone,
            SalaryBand,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
