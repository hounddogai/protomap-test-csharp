namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility request is in its life cycle.</summary>
public enum MobilityRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A mobility request. Its code is unique within a tenant.</summary>
public sealed class MobilityRequest(string code, string label, int createdAt, double threshold, string channel, string location, string department, double rate, double minimum, double fte, MobilityRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public double Threshold { get; set; } = threshold;
    public string Channel { get; set; } = channel;
    public string Location { get; set; } = location;
    public string Department { get; set; } = department;
    public double Rate { get; set; } = rate;
    public double Minimum { get; set; } = minimum;
    public double Fte { get; set; } = fte;
    public MobilityRequestStatus Status { get; set; } = status;

    /// <summary>Starts a mobility request in status draft.</summary>
    public static MobilityRequest NewMobilityRequest(string code, string label)
    {
        return new MobilityRequest(code, label, 0, 0.0, "", "", "", 0.0, 0.0, 0.0, MobilityRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityRequestStatus next)
    {
        switch (Status)
        {
            case MobilityRequestStatus.Draft:
                return next == MobilityRequestStatus.Suspended || next == MobilityRequestStatus.Closed;
            case MobilityRequestStatus.Pending:
                switch (next)
                {
                    case MobilityRequestStatus.Closed:
                        return true;
                }
                return false;
            case MobilityRequestStatus.Active:
                return next == MobilityRequestStatus.Suspended || next == MobilityRequestStatus.Closed;
            case MobilityRequestStatus.Suspended:
                switch (next)
                {
                    case MobilityRequestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedValue()
    {
        return Math.Round(Threshold + Minimum, 2);
    }

    public MobilityRequest Copy()
    {
        return new MobilityRequest(
            Code,
            Label,
            CreatedAt,
            Threshold,
            Channel,
            Location,
            Department,
            Rate,
            Minimum,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
