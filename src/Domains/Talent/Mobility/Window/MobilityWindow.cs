namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility window is in its life cycle.</summary>
public enum MobilityWindowStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One mobility window, with what the planners need to act on it.</summary>
public sealed class MobilityWindow(string code, string label, bool visible, double maximum, string segment, int graceDays, int windowDays, double budget, MobilityWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public double Maximum { get; set; } = maximum;
    public string Segment { get; set; } = segment;
    public int GraceDays { get; set; } = graceDays;
    public int WindowDays { get; set; } = windowDays;
    public double Budget { get; set; } = budget;
    public MobilityWindowStatus Status { get; set; } = status;

    /// <summary>Starts a mobility window in status new.</summary>
    public static MobilityWindow NewMobilityWindow(string code, string label)
    {
        return new MobilityWindow(code, label, false, 0.0, "", 0, 0, 0.0, MobilityWindowStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityWindowStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityWindowStatus next)
    {
        switch (Status)
        {
            case MobilityWindowStatus.New:
                return next == MobilityWindowStatus.Queued || next == MobilityWindowStatus.Running;
            case MobilityWindowStatus.Queued:
                switch (next)
                {
                    case MobilityWindowStatus.Done:
                        return true;
                }
                return false;
            case MobilityWindowStatus.Running:
                switch (next)
                {
                    case MobilityWindowStatus.Done:
                        return true;
                }
                return false;
            case MobilityWindowStatus.Done:
                switch (next)
                {
                    case MobilityWindowStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedLoad()
    {
        return Math.Round(Maximum * Budget, 2);
    }

    public MobilityWindow Copy()
    {
        return new MobilityWindow(Code, Label, Visible, Maximum, Segment, GraceDays, WindowDays, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
