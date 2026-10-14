namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending scenario request is in its life cycle.</summary>
public enum PendingScenarioRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a pending scenario request and its current status.</summary>
public sealed class PendingScenarioRequest(string code, string label, int windowDays, string description, double threshold, int sequence, string phone, PendingScenarioRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string Description { get; set; } = description;
    public double Threshold { get; set; } = threshold;
    public int Sequence { get; set; } = sequence;
    public string Phone { get; set; } = phone;
    public PendingScenarioRequestStatus Status { get; set; } = status;

    /// <summary>Starts a pending scenario request in status planned.</summary>
    public static PendingScenarioRequest NewPendingScenarioRequest(string code, string label)
    {
        return new PendingScenarioRequest(code, label, 0, "", 0.0, 0, "", PendingScenarioRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingScenarioRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingScenarioRequestStatus next)
    {
        switch (Status)
        {
            case PendingScenarioRequestStatus.Planned:
                return next == PendingScenarioRequestStatus.Scheduled;
            case PendingScenarioRequestStatus.Scheduled:
                switch (next)
                {
                    case PendingScenarioRequestStatus.InProgress:
                    case PendingScenarioRequestStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingScenarioRequestStatus.InProgress:
                return next == PendingScenarioRequestStatus.Cancelled;
            case PendingScenarioRequestStatus.Completed:
                return next == PendingScenarioRequestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PendingScenarioRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending scenario request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pending scenario request"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Threshold - (double)WindowDays, 2);
    }

    public PendingScenarioRequest Copy()
    {
        return new PendingScenarioRequest(Code, Label, WindowDays, Description, Threshold, Sequence, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
