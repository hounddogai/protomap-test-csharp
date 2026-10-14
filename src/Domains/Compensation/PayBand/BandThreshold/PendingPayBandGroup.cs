namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending pay band group is in its life cycle.</summary>
public enum PendingPayBandGroupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pending pay band group as the pay band team keeps it.</summary>
public sealed class PendingPayBandGroup(string code, string label, string jobCode, bool archived, double weight, string name, PendingPayBandGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public bool Archived { get; set; } = archived;
    public double Weight { get; set; } = weight;
    public string Name { get; set; } = name;
    public PendingPayBandGroupStatus Status { get; set; } = status;

    /// <summary>Starts a pending pay band group in status draft.</summary>
    public static PendingPayBandGroup NewPendingPayBandGroup(string code, string label)
    {
        return new PendingPayBandGroup(code, label, "", false, 0.0, "", PendingPayBandGroupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPayBandGroupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPayBandGroupStatus next)
    {
        switch (Status)
        {
            case PendingPayBandGroupStatus.Draft:
                switch (next)
                {
                    case PendingPayBandGroupStatus.Pending:
                    case PendingPayBandGroupStatus.Suspended:
                        return true;
                }
                return false;
            case PendingPayBandGroupStatus.Pending:
                return next == PendingPayBandGroupStatus.Active;
            case PendingPayBandGroupStatus.Active:
                return next == PendingPayBandGroupStatus.Suspended || next == PendingPayBandGroupStatus.Closed;
            case PendingPayBandGroupStatus.Suspended:
                return next == PendingPayBandGroupStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PendingPayBandGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending pay band group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pending pay band group"} {Code}";
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public PendingPayBandGroup Copy()
    {
        return new PendingPayBandGroup(Code, Label, JobCode, Archived, Weight, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
