namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled retro pay checkpoint is in its life cycle.</summary>
public enum ScheduledRetroPayCheckpointStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A scheduled retro pay checkpoint as the retro pay team keeps it.</summary>
public sealed class ScheduledRetroPayCheckpoint(string code, string label, int priority, string currency, bool approved, string locale, string ownerId, string costCenter, ScheduledRetroPayCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Currency { get; set; } = currency;
    public bool Approved { get; set; } = approved;
    public string Locale { get; set; } = locale;
    public string OwnerId { get; set; } = ownerId;
    public string CostCenter { get; set; } = costCenter;
    public ScheduledRetroPayCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled retro pay checkpoint in status new.</summary>
    public static ScheduledRetroPayCheckpoint NewScheduledRetroPayCheckpoint(string code, string label)
    {
        return new ScheduledRetroPayCheckpoint(
            code,
            label,
            0,
            "",
            false,
            "",
            "",
            "",
            ScheduledRetroPayCheckpointStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledRetroPayCheckpointStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledRetroPayCheckpointStatus next)
    {
        switch (Status)
        {
            case ScheduledRetroPayCheckpointStatus.New:
                switch (next)
                {
                    case ScheduledRetroPayCheckpointStatus.Queued:
                        return true;
                }
                return false;
            case ScheduledRetroPayCheckpointStatus.Queued:
                return next == ScheduledRetroPayCheckpointStatus.Running || next == ScheduledRetroPayCheckpointStatus.Done;
            case ScheduledRetroPayCheckpointStatus.Running:
                return next == ScheduledRetroPayCheckpointStatus.Done || next == ScheduledRetroPayCheckpointStatus.Failed;
            case ScheduledRetroPayCheckpointStatus.Done:
                return next == ScheduledRetroPayCheckpointStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ScheduledRetroPayCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled retro pay checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public ScheduledRetroPayCheckpoint Copy()
    {
        return new ScheduledRetroPayCheckpoint(
            Code,
            Label,
            Priority,
            Currency,
            Approved,
            Locale,
            OwnerId,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
