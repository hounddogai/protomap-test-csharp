namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due plan is in its life cycle.</summary>
public enum TrainingDuePlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a training due plan and its current status.</summary>
public sealed class TrainingDuePlan(string code, string name, double minimum, bool visible, double rating, int leaveDays, TrainingDuePlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public bool Visible { get; set; } = visible;
    public double Rating { get; set; } = rating;
    public int LeaveDays { get; set; } = leaveDays;
    public TrainingDuePlanStatus Status { get; set; } = status;

    /// <summary>Starts a training due plan in status draft.</summary>
    public static TrainingDuePlan NewTrainingDuePlan(string code, string name)
    {
        return new TrainingDuePlan(code, name, 0.0, false, 0.0, 0, TrainingDuePlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDuePlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDuePlanStatus next)
    {
        switch (Status)
        {
            case TrainingDuePlanStatus.Draft:
                switch (next)
                {
                    case TrainingDuePlanStatus.Active:
                        return true;
                }
                return false;
            case TrainingDuePlanStatus.Pending:
                return next == TrainingDuePlanStatus.Suspended || next == TrainingDuePlanStatus.Closed;
            case TrainingDuePlanStatus.Active:
                switch (next)
                {
                    case TrainingDuePlanStatus.Suspended:
                        return true;
                }
                return false;
            case TrainingDuePlanStatus.Suspended:
                return next == TrainingDuePlanStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TrainingDuePlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetTotal()
    {
        return Math.Round(Minimum * Rating, 2);
    }

    public TrainingDuePlan Copy()
    {
        return new TrainingDuePlan(Code, Name, Minimum, Visible, Rating, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
