namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle exception is in its life cycle.</summary>
public enum MeritCycleExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One merit cycle exception, with what the planners need to act on it.</summary>
public sealed class MeritCycleException(string code, string title, string checksum, bool active, string notes, double hours, string phone, int startsAt, string name, double amount, bool pinned, MeritCycleExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public bool Active { get; set; } = active;
    public string Notes { get; set; } = notes;
    public double Hours { get; set; } = hours;
    public string Phone { get; set; } = phone;
    public int StartsAt { get; set; } = startsAt;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public bool Pinned { get; set; } = pinned;
    public MeritCycleExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle exception in status draft.</summary>
    public static MeritCycleException NewMeritCycleException(string code, string title)
    {
        return new MeritCycleException(
            code,
            title,
            "",
            false,
            "",
            0.0,
            "",
            0,
            "",
            0.0,
            false,
            MeritCycleExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleExceptionStatus next)
    {
        switch (Status)
        {
            case MeritCycleExceptionStatus.Draft:
                switch (next)
                {
                    case MeritCycleExceptionStatus.Suspended:
                    case MeritCycleExceptionStatus.Closed:
                        return true;
                }
                return false;
            case MeritCycleExceptionStatus.Pending:
                switch (next)
                {
                    case MeritCycleExceptionStatus.Suspended:
                        return true;
                }
                return false;
            case MeritCycleExceptionStatus.Active:
                return next == MeritCycleExceptionStatus.Suspended || next == MeritCycleExceptionStatus.Closed;
            case MeritCycleExceptionStatus.Suspended:
                switch (next)
                {
                    case MeritCycleExceptionStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)StartsAt - Hours, 2);
    }

    public MeritCycleException Copy()
    {
        return new MeritCycleException(
            Code,
            Title,
            Checksum,
            Active,
            Notes,
            Hours,
            Phone,
            StartsAt,
            Name,
            Amount,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
