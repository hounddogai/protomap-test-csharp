namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback visitor cycle is in its life cycle.</summary>
public enum FallbackVisitorCycleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a fallback visitor cycle and its current status.</summary>
public sealed class FallbackVisitorCycle(string code, string name, int endsAt, string locale, string jobCode, int startsAt, string grade, double rating, int sequence, string managerId, FallbackVisitorCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public string Locale { get; set; } = locale;
    public string JobCode { get; set; } = jobCode;
    public int StartsAt { get; set; } = startsAt;
    public string Grade { get; set; } = grade;
    public double Rating { get; set; } = rating;
    public int Sequence { get; set; } = sequence;
    public string ManagerId { get; set; } = managerId;
    public FallbackVisitorCycleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback visitor cycle in status draft.</summary>
    public static FallbackVisitorCycle NewFallbackVisitorCycle(string code, string name)
    {
        return new FallbackVisitorCycle(code, name, 0, "", "", 0, "", 0.0, 0, "", FallbackVisitorCycleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackVisitorCycleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackVisitorCycleStatus next)
    {
        switch (Status)
        {
            case FallbackVisitorCycleStatus.Draft:
                return next == FallbackVisitorCycleStatus.Pending;
            case FallbackVisitorCycleStatus.Pending:
                return next == FallbackVisitorCycleStatus.Active || next == FallbackVisitorCycleStatus.Suspended;
            case FallbackVisitorCycleStatus.Active:
                switch (next)
                {
                    case FallbackVisitorCycleStatus.Closed:
                        return true;
                }
                return false;
            case FallbackVisitorCycleStatus.Suspended:
                switch (next)
                {
                    case FallbackVisitorCycleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackVisitorCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback visitor cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)StartsAt + (double)EndsAt, 2);
    }

    public FallbackVisitorCycle Copy()
    {
        return new FallbackVisitorCycle(
            Code,
            Name,
            EndsAt,
            Locale,
            JobCode,
            StartsAt,
            Grade,
            Rating,
            Sequence,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
