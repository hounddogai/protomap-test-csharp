namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient mobility request is in its life cycle.</summary>
public enum LenientMobilityRequestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One lenient mobility request, with what the planners need to act on it.</summary>
public sealed class LenientMobilityRequest(string code, string name, string costCenter, bool pinned, double ratio, string phone, string checksum, int priority, string jobCode, double fte, LenientMobilityRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public bool Pinned { get; set; } = pinned;
    public double Ratio { get; set; } = ratio;
    public string Phone { get; set; } = phone;
    public string Checksum { get; set; } = checksum;
    public int Priority { get; set; } = priority;
    public string JobCode { get; set; } = jobCode;
    public double Fte { get; set; } = fte;
    public LenientMobilityRequestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient mobility request in status requested.</summary>
    public static LenientMobilityRequest NewLenientMobilityRequest(string code, string name)
    {
        return new LenientMobilityRequest(
            code,
            name,
            "",
            false,
            0.0,
            "",
            "",
            0,
            "",
            0.0,
            LenientMobilityRequestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientMobilityRequestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientMobilityRequestStatus next)
    {
        switch (Status)
        {
            case LenientMobilityRequestStatus.Requested:
                switch (next)
                {
                    case LenientMobilityRequestStatus.Revoked:
                        return true;
                }
                return false;
            case LenientMobilityRequestStatus.Granted:
                return next == LenientMobilityRequestStatus.Expired;
            case LenientMobilityRequestStatus.Revoked:
                return next == LenientMobilityRequestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LenientMobilityRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient mobility request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient mobility request"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Fte - (double)Priority, 2);
    }

    public LenientMobilityRequest Copy()
    {
        return new LenientMobilityRequest(
            Code,
            Name,
            CostCenter,
            Pinned,
            Ratio,
            Phone,
            Checksum,
            Priority,
            JobCode,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
