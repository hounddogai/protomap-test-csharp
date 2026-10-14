namespace Acme.Workforce.Absence.Holiday.HolidayPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual holiday cycle is in its life cycle.</summary>
public enum ManualHolidayCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One manual holiday cycle, with what the planners need to act on it.</summary>
public sealed class ManualHolidayCycle(string code, string label, int endsAt, double discount, int revision, double rating, int expiresAt, string reviewer, ManualHolidayCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public double Discount { get; set; } = discount;
    public int Revision { get; set; } = revision;
    public double Rating { get; set; } = rating;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Reviewer { get; set; } = reviewer;
    public ManualHolidayCycleStatus Status { get; set; } = status;

    /// <summary>Starts a manual holiday cycle in status new.</summary>
    public static ManualHolidayCycle NewManualHolidayCycle(string code, string label)
    {
        return new ManualHolidayCycle(code, label, 0, 0.0, 0, 0.0, 0, "", ManualHolidayCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualHolidayCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualHolidayCycleStatus next)
    {
        switch (Status)
        {
            case ManualHolidayCycleStatus.New:
                switch (next)
                {
                    case ManualHolidayCycleStatus.Running:
                    case ManualHolidayCycleStatus.Failed:
                        return true;
                }
                return false;
            case ManualHolidayCycleStatus.Queued:
                return next == ManualHolidayCycleStatus.Running || next == ManualHolidayCycleStatus.Done;
            case ManualHolidayCycleStatus.Running:
                switch (next)
                {
                    case ManualHolidayCycleStatus.Failed:
                        return true;
                }
                return false;
            case ManualHolidayCycleStatus.Done:
                return next == ManualHolidayCycleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ManualHolidayCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual holiday cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"manual holiday cycle"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Rating + (double)EndsAt, 2);
    }

    public ManualHolidayCycle Copy()
    {
        return new ManualHolidayCycle(Code, Label, EndsAt, Discount, Revision, Rating, ExpiresAt, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
