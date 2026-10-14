namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict rate notice is in its life cycle.</summary>
public enum StrictRateNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A strict rate notice as the rate team keeps it.</summary>
public sealed class StrictRateNotice(string code, string name, int endsAt, double minimum, bool approved, int createdAt, StrictRateNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public double Minimum { get; set; } = minimum;
    public bool Approved { get; set; } = approved;
    public int CreatedAt { get; set; } = createdAt;
    public StrictRateNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a strict rate notice in status new.</summary>
    public static StrictRateNotice NewStrictRateNotice(string code, string name)
    {
        return new StrictRateNotice(code, name, 0, 0.0, false, 0, StrictRateNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictRateNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictRateNoticeStatus next)
    {
        switch (Status)
        {
            case StrictRateNoticeStatus.New:
                switch (next)
                {
                    case StrictRateNoticeStatus.Running:
                    case StrictRateNoticeStatus.Done:
                        return true;
                }
                return false;
            case StrictRateNoticeStatus.Queued:
                switch (next)
                {
                    case StrictRateNoticeStatus.Done:
                        return true;
                }
                return false;
            case StrictRateNoticeStatus.Running:
                switch (next)
                {
                    case StrictRateNoticeStatus.Failed:
                        return true;
                }
                return false;
            case StrictRateNoticeStatus.Done:
                switch (next)
                {
                    case StrictRateNoticeStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictRateNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict rate notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"strict rate notice"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)EndsAt + (double)CreatedAt, 2);
    }

    public StrictRateNotice Copy()
    {
        return new StrictRateNotice(Code, Name, EndsAt, Minimum, Approved, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
