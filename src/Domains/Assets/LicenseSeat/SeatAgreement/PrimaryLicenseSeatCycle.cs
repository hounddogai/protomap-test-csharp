namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary license seat cycle is in its life cycle.</summary>
public enum PrimaryLicenseSeatCycleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One primary license seat cycle, with what the planners need to act on it.</summary>
public sealed class PrimaryLicenseSeatCycle(string code, string name, string summary, bool approved, string department, int startsAt, string contactEmail, double weight, string channel, double minimum, PrimaryLicenseSeatCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public bool Approved { get; set; } = approved;
    public string Department { get; set; } = department;
    public int StartsAt { get; set; } = startsAt;
    public string ContactEmail { get; set; } = contactEmail;
    public double Weight { get; set; } = weight;
    public string Channel { get; set; } = channel;
    public double Minimum { get; set; } = minimum;
    public PrimaryLicenseSeatCycleStatus Status { get; set; } = status;

    /// <summary>Starts a primary license seat cycle in status idle.</summary>
    public static PrimaryLicenseSeatCycle NewPrimaryLicenseSeatCycle(string code, string name)
    {
        return new PrimaryLicenseSeatCycle(
            code,
            name,
            "",
            false,
            "",
            0,
            "",
            0.0,
            "",
            0.0,
            PrimaryLicenseSeatCycleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryLicenseSeatCycleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryLicenseSeatCycleStatus next)
    {
        switch (Status)
        {
            case PrimaryLicenseSeatCycleStatus.Idle:
                switch (next)
                {
                    case PrimaryLicenseSeatCycleStatus.Ready:
                        return true;
                }
                return false;
            case PrimaryLicenseSeatCycleStatus.Warming:
                return next == PrimaryLicenseSeatCycleStatus.Stopped;
            case PrimaryLicenseSeatCycleStatus.Ready:
                switch (next)
                {
                    case PrimaryLicenseSeatCycleStatus.Draining:
                        return true;
                }
                return false;
            case PrimaryLicenseSeatCycleStatus.Draining:
                switch (next)
                {
                    case PrimaryLicenseSeatCycleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryLicenseSeatCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary license seat cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)StartsAt + Weight, 2);
    }

    public PrimaryLicenseSeatCycle Copy()
    {
        return new PrimaryLicenseSeatCycle(
            Code,
            Name,
            Summary,
            Approved,
            Department,
            StartsAt,
            ContactEmail,
            Weight,
            Channel,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
