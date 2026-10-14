namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict merit cycle estimate is in its life cycle.</summary>
public enum StrictMeritCycleEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One strict merit cycle estimate, with what the planners need to act on it.</summary>
public sealed class StrictMeritCycleEstimate(string code, string label, string notes, string location, List<string> tags, double amount, StrictMeritCycleEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string Location { get; set; } = location;
    public List<string> Tags { get; set; } = tags;
    public double Amount { get; set; } = amount;
    public StrictMeritCycleEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a strict merit cycle estimate in status requested.</summary>
    public static StrictMeritCycleEstimate NewStrictMeritCycleEstimate(string code, string label)
    {
        return new StrictMeritCycleEstimate(
            code,
            label,
            "",
            "",
            new List<string>(),
            0.0,
            StrictMeritCycleEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictMeritCycleEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictMeritCycleEstimateStatus next)
    {
        switch (Status)
        {
            case StrictMeritCycleEstimateStatus.Requested:
                switch (next)
                {
                    case StrictMeritCycleEstimateStatus.Revoked:
                        return true;
                }
                return false;
            case StrictMeritCycleEstimateStatus.Granted:
                switch (next)
                {
                    case StrictMeritCycleEstimateStatus.Revoked:
                    case StrictMeritCycleEstimateStatus.Expired:
                        return true;
                }
                return false;
            case StrictMeritCycleEstimateStatus.Revoked:
                switch (next)
                {
                    case StrictMeritCycleEstimateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictMeritCycleEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict merit cycle estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"strict merit cycle estimate"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public StrictMeritCycleEstimate Copy()
    {
        return new StrictMeritCycleEstimate(Code, Label, Notes, Location, Tags, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
