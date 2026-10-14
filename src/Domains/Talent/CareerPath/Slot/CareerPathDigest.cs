namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path digest is in its life cycle.</summary>
public enum CareerPathDigestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One career path digest, with what the planners need to act on it.</summary>
public sealed class CareerPathDigest(string code, string name, string region, double fte, string team, string ownerId, int quantity, string channel, CareerPathDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public double Fte { get; set; } = fte;
    public string Team { get; set; } = team;
    public string OwnerId { get; set; } = ownerId;
    public int Quantity { get; set; } = quantity;
    public string Channel { get; set; } = channel;
    public CareerPathDigestStatus Status { get; set; } = status;

    /// <summary>Starts a career path digest in status draft.</summary>
    public static CareerPathDigest NewCareerPathDigest(string code, string name)
    {
        return new CareerPathDigest(code, name, "", 0.0, "", "", 0, "", CareerPathDigestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathDigestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathDigestStatus next)
    {
        switch (Status)
        {
            case CareerPathDigestStatus.Draft:
                switch (next)
                {
                    case CareerPathDigestStatus.Pending:
                    case CareerPathDigestStatus.Suspended:
                        return true;
                }
                return false;
            case CareerPathDigestStatus.Pending:
                return next == CareerPathDigestStatus.Active || next == CareerPathDigestStatus.Closed;
            case CareerPathDigestStatus.Active:
                return next == CareerPathDigestStatus.Suspended || next == CareerPathDigestStatus.Closed;
            case CareerPathDigestStatus.Suspended:
                switch (next)
                {
                    case CareerPathDigestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Fte + (double)Quantity, 2);
    }

    public CareerPathDigest Copy()
    {
        return new CareerPathDigest(Code, Name, Region, Fte, Team, OwnerId, Quantity, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
