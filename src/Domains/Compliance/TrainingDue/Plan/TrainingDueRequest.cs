namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due request is in its life cycle.</summary>
public enum TrainingDueRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a training due request and its current status.</summary>
public sealed class TrainingDueRequest(string code, string title, int retries, double maximum, string ownerId, double discount, double fte, int windowDays, bool visible, double hours, string name, TrainingDueRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public double Maximum { get; set; } = maximum;
    public string OwnerId { get; set; } = ownerId;
    public double Discount { get; set; } = discount;
    public double Fte { get; set; } = fte;
    public int WindowDays { get; set; } = windowDays;
    public bool Visible { get; set; } = visible;
    public double Hours { get; set; } = hours;
    public string Name { get; set; } = name;
    public TrainingDueRequestStatus Status { get; set; } = status;

    /// <summary>Starts a training due request in status new.</summary>
    public static TrainingDueRequest NewTrainingDueRequest(string code, string title)
    {
        return new TrainingDueRequest(
            code,
            title,
            0,
            0.0,
            "",
            0.0,
            0.0,
            0,
            false,
            0.0,
            "",
            TrainingDueRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueRequestStatus next)
    {
        switch (Status)
        {
            case TrainingDueRequestStatus.New:
                return next == TrainingDueRequestStatus.Failed;
            case TrainingDueRequestStatus.Queued:
                return next == TrainingDueRequestStatus.Done;
            case TrainingDueRequestStatus.Running:
                switch (next)
                {
                    case TrainingDueRequestStatus.Done:
                    case TrainingDueRequestStatus.Failed:
                        return true;
                }
                return false;
            case TrainingDueRequestStatus.Done:
                return next == TrainingDueRequestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TrainingDueRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Fte - Hours, 2);
    }

    public TrainingDueRequest Copy()
    {
        return new TrainingDueRequest(
            Code,
            Title,
            Retries,
            Maximum,
            OwnerId,
            Discount,
            Fte,
            WindowDays,
            Visible,
            Hours,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
