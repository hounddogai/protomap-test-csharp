namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position variance is in its life cycle.</summary>
public enum PositionVarianceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One position variance, with what the planners need to act on it.</summary>
public sealed class PositionVariance(string code, string label, int windowDays, double weight, int revision, int endsAt, double hours, PositionVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public double Weight { get; set; } = weight;
    public int Revision { get; set; } = revision;
    public int EndsAt { get; set; } = endsAt;
    public double Hours { get; set; } = hours;
    public PositionVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a position variance in status planned.</summary>
    public static PositionVariance NewPositionVariance(string code, string label)
    {
        return new PositionVariance(code, label, 0, 0.0, 0, 0, 0.0, PositionVarianceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionVarianceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionVarianceStatus next)
    {
        switch (Status)
        {
            case PositionVarianceStatus.Planned:
                return next == PositionVarianceStatus.Cancelled;
            case PositionVarianceStatus.Scheduled:
                switch (next)
                {
                    case PositionVarianceStatus.Completed:
                    case PositionVarianceStatus.Cancelled:
                        return true;
                }
                return false;
            case PositionVarianceStatus.InProgress:
                return next == PositionVarianceStatus.Cancelled;
            case PositionVarianceStatus.Completed:
                return next == PositionVarianceStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PositionVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Revision - Weight, 2);
    }

    public PositionVariance Copy()
    {
        return new PositionVariance(Code, Label, WindowDays, Weight, Revision, EndsAt, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
