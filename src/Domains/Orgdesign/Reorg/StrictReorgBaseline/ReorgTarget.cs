namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg target is in its life cycle.</summary>
public enum ReorgTargetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a reorg target and its current status.</summary>
public sealed class ReorgTarget(string code, string title, string jobCode, double floorValue, int retries, string badge, int sequence, string locale, double fte, bool archived, string notes, ReorgTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public double FloorValue { get; set; } = floorValue;
    public int Retries { get; set; } = retries;
    public string Badge { get; set; } = badge;
    public int Sequence { get; set; } = sequence;
    public string Locale { get; set; } = locale;
    public double Fte { get; set; } = fte;
    public bool Archived { get; set; } = archived;
    public string Notes { get; set; } = notes;
    public ReorgTargetStatus Status { get; set; } = status;

    /// <summary>Starts a reorg target in status idle.</summary>
    public static ReorgTarget NewReorgTarget(string code, string title)
    {
        return new ReorgTarget(code, title, "", 0.0, 0, "", 0, "", 0.0, false, "", ReorgTargetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgTargetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgTargetStatus next)
    {
        switch (Status)
        {
            case ReorgTargetStatus.Idle:
                switch (next)
                {
                    case ReorgTargetStatus.Warming:
                        return true;
                }
                return false;
            case ReorgTargetStatus.Warming:
                switch (next)
                {
                    case ReorgTargetStatus.Draining:
                        return true;
                }
                return false;
            case ReorgTargetStatus.Ready:
                return next == ReorgTargetStatus.Draining;
            case ReorgTargetStatus.Draining:
                switch (next)
                {
                    case ReorgTargetStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Retries + (double)Sequence, 2);
    }

    public ReorgTarget Copy()
    {
        return new ReorgTarget(
            Code,
            Title,
            JobCode,
            FloorValue,
            Retries,
            Badge,
            Sequence,
            Locale,
            Fte,
            Archived,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
