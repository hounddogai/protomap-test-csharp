namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path draft is in its life cycle.</summary>
public enum CareerPathDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One career path draft, with what the planners need to act on it.</summary>
public sealed class CareerPathDraft(string code, string label, int sequence, string contactEmail, string segment, int version, string department, double amount, string name, double fte, CareerPathDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public string ContactEmail { get; set; } = contactEmail;
    public string Segment { get; set; } = segment;
    public int Version { get; set; } = version;
    public string Department { get; set; } = department;
    public double Amount { get; set; } = amount;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public CareerPathDraftStatus Status { get; set; } = status;

    /// <summary>Starts a career path draft in status planned.</summary>
    public static CareerPathDraft NewCareerPathDraft(string code, string label)
    {
        return new CareerPathDraft(code, label, 0, "", "", 0, "", 0.0, "", 0.0, CareerPathDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathDraftStatus next)
    {
        switch (Status)
        {
            case CareerPathDraftStatus.Planned:
                switch (next)
                {
                    case CareerPathDraftStatus.Cancelled:
                        return true;
                }
                return false;
            case CareerPathDraftStatus.Scheduled:
                return next == CareerPathDraftStatus.Cancelled;
            case CareerPathDraftStatus.InProgress:
                return next == CareerPathDraftStatus.Completed;
            case CareerPathDraftStatus.Completed:
                return next == CareerPathDraftStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CareerPathDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Version * (double)Sequence, 2);
    }

    public CareerPathDraft Copy()
    {
        return new CareerPathDraft(
            Code,
            Label,
            Sequence,
            ContactEmail,
            Segment,
            Version,
            Department,
            Amount,
            Name,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
