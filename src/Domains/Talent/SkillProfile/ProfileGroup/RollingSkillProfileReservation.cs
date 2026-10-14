namespace Acme.Workforce.Talent.SkillProfile.ProfileGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling skill profile reservation is in its life cycle.</summary>
public enum RollingSkillProfileReservationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A rolling skill profile reservation. Its code is unique within a tenant.</summary>
public sealed class RollingSkillProfileReservation(string code, string title, string phone, bool pinned, string label, List<string> tags, double cost, string costCenter, double margin, bool visible, string contactEmail, RollingSkillProfileReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public bool Pinned { get; set; } = pinned;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public double Cost { get; set; } = cost;
    public string CostCenter { get; set; } = costCenter;
    public double Margin { get; set; } = margin;
    public bool Visible { get; set; } = visible;
    public string ContactEmail { get; set; } = contactEmail;
    public RollingSkillProfileReservationStatus Status { get; set; } = status;

    /// <summary>Starts a rolling skill profile reservation in status planned.</summary>
    public static RollingSkillProfileReservation NewRollingSkillProfileReservation(string code, string title)
    {
        return new RollingSkillProfileReservation(
            code,
            title,
            "",
            false,
            "",
            new List<string>(),
            0.0,
            "",
            0.0,
            false,
            "",
            RollingSkillProfileReservationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingSkillProfileReservationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingSkillProfileReservationStatus next)
    {
        switch (Status)
        {
            case RollingSkillProfileReservationStatus.Planned:
                return next == RollingSkillProfileReservationStatus.Scheduled;
            case RollingSkillProfileReservationStatus.Scheduled:
                return next == RollingSkillProfileReservationStatus.InProgress;
            case RollingSkillProfileReservationStatus.InProgress:
                switch (next)
                {
                    case RollingSkillProfileReservationStatus.Completed:
                        return true;
                }
                return false;
            case RollingSkillProfileReservationStatus.Completed:
                switch (next)
                {
                    case RollingSkillProfileReservationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingSkillProfileReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling skill profile reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
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

    public double EffectiveExposure()
    {
        return Math.Round(Margin * Cost, 2);
    }

    public RollingSkillProfileReservation Copy()
    {
        return new RollingSkillProfileReservation(
            Code,
            Title,
            Phone,
            Pinned,
            Label,
            Tags,
            Cost,
            CostCenter,
            Margin,
            Visible,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
