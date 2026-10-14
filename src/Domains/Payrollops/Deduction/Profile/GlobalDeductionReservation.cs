namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global deduction reservation is in its life cycle.</summary>
public enum GlobalDeductionReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One global deduction reservation, with what the planners need to act on it.</summary>
public sealed class GlobalDeductionReservation(string code, string title, string segment, double score, string category, double amount, double weight, double rating, GlobalDeductionReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public string Category { get; set; } = category;
    public double Amount { get; set; } = amount;
    public double Weight { get; set; } = weight;
    public double Rating { get; set; } = rating;
    public GlobalDeductionReservationStatus Status { get; set; } = status;

    /// <summary>Starts a global deduction reservation in status idle.</summary>
    public static GlobalDeductionReservation NewGlobalDeductionReservation(string code, string title)
    {
        return new GlobalDeductionReservation(
            code,
            title,
            "",
            0.0,
            "",
            0.0,
            0.0,
            0.0,
            GlobalDeductionReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalDeductionReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalDeductionReservationStatus next)
    {
        switch (Status)
        {
            case GlobalDeductionReservationStatus.Idle:
                switch (next)
                {
                    case GlobalDeductionReservationStatus.Ready:
                        return true;
                }
                return false;
            case GlobalDeductionReservationStatus.Warming:
                switch (next)
                {
                    case GlobalDeductionReservationStatus.Ready:
                        return true;
                }
                return false;
            case GlobalDeductionReservationStatus.Ready:
                return next == GlobalDeductionReservationStatus.Stopped;
            case GlobalDeductionReservationStatus.Draining:
                return next == GlobalDeductionReservationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(GlobalDeductionReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global deduction reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global deduction reservation"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Score * Weight, 2);
    }

    public GlobalDeductionReservation Copy()
    {
        return new GlobalDeductionReservation(Code, Title, Segment, Score, Category, Amount, Weight, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
