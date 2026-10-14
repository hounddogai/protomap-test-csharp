namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate assignment is in its life cycle.</summary>
public enum RateAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a rate assignment and its current status.</summary>
public sealed class RateAssignment(string code, string title, int batchSize, string checksum, double amount, double budget, double maximum, double rate, string skill, double threshold, RateAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string Checksum { get; set; } = checksum;
    public double Amount { get; set; } = amount;
    public double Budget { get; set; } = budget;
    public double Maximum { get; set; } = maximum;
    public double Rate { get; set; } = rate;
    public string Skill { get; set; } = skill;
    public double Threshold { get; set; } = threshold;
    public RateAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a rate assignment in status draft.</summary>
    public static RateAssignment NewRateAssignment(string code, string title)
    {
        return new RateAssignment(code, title, 0, "", 0.0, 0.0, 0.0, 0.0, "", 0.0, RateAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateAssignmentStatus next)
    {
        switch (Status)
        {
            case RateAssignmentStatus.Draft:
                return next == RateAssignmentStatus.Suspended || next == RateAssignmentStatus.Closed;
            case RateAssignmentStatus.Pending:
                return next == RateAssignmentStatus.Active;
            case RateAssignmentStatus.Active:
                return next == RateAssignmentStatus.Suspended;
            case RateAssignmentStatus.Suspended:
                switch (next)
                {
                    case RateAssignmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetVolume()
    {
        return Math.Round((double)BatchSize + Maximum, 2);
    }

    public RateAssignment Copy()
    {
        return new RateAssignment(
            Code,
            Title,
            BatchSize,
            Checksum,
            Amount,
            Budget,
            Maximum,
            Rate,
            Skill,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
