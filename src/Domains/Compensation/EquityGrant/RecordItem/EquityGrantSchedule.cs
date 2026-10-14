namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant schedule is in its life cycle.</summary>
public enum EquityGrantScheduleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a equity grant schedule and its current status.</summary>
public sealed class EquityGrantSchedule(string code, string title, string phone, bool visible, double weight, double margin, EquityGrantScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public bool Visible { get; set; } = visible;
    public double Weight { get; set; } = weight;
    public double Margin { get; set; } = margin;
    public EquityGrantScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant schedule in status idle.</summary>
    public static EquityGrantSchedule NewEquityGrantSchedule(string code, string title)
    {
        return new EquityGrantSchedule(code, title, "", false, 0.0, 0.0, EquityGrantScheduleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantScheduleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantScheduleStatus next)
    {
        switch (Status)
        {
            case EquityGrantScheduleStatus.Idle:
                return next == EquityGrantScheduleStatus.Draining;
            case EquityGrantScheduleStatus.Warming:
                return next == EquityGrantScheduleStatus.Draining;
            case EquityGrantScheduleStatus.Ready:
                switch (next)
                {
                    case EquityGrantScheduleStatus.Draining:
                    case EquityGrantScheduleStatus.Stopped:
                        return true;
                }
                return false;
            case EquityGrantScheduleStatus.Draining:
                switch (next)
                {
                    case EquityGrantScheduleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"equity grant schedule"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Weight * Margin, 2);
    }

    public EquityGrantSchedule Copy()
    {
        return new EquityGrantSchedule(Code, Title, Phone, Visible, Weight, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
