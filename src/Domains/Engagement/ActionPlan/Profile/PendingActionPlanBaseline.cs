namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending action plan baseline is in its life cycle.</summary>
public enum PendingActionPlanBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pending action plan baseline and its current status.</summary>
public sealed class PendingActionPlanBaseline(string code, string name, int windowDays, int revision, int attempts, int endsAt, string label, string employeeId, double minimum, PendingActionPlanBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public int Revision { get; set; } = revision;
    public int Attempts { get; set; } = attempts;
    public int EndsAt { get; set; } = endsAt;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public double Minimum { get; set; } = minimum;
    public PendingActionPlanBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a pending action plan baseline in status created.</summary>
    public static PendingActionPlanBaseline NewPendingActionPlanBaseline(string code, string name)
    {
        return new PendingActionPlanBaseline(
            code,
            name,
            0,
            0,
            0,
            0,
            "",
            "",
            0.0,
            PendingActionPlanBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingActionPlanBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingActionPlanBaselineStatus next)
    {
        switch (Status)
        {
            case PendingActionPlanBaselineStatus.Created:
                switch (next)
                {
                    case PendingActionPlanBaselineStatus.Validated:
                    case PendingActionPlanBaselineStatus.Retired:
                        return true;
                }
                return false;
            case PendingActionPlanBaselineStatus.Validated:
                switch (next)
                {
                    case PendingActionPlanBaselineStatus.Published:
                    case PendingActionPlanBaselineStatus.Retired:
                        return true;
                }
                return false;
            case PendingActionPlanBaselineStatus.Published:
                switch (next)
                {
                    case PendingActionPlanBaselineStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingActionPlanBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending action plan baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"pending action plan baseline"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)EndsAt * Minimum, 2);
    }

    public PendingActionPlanBaseline Copy()
    {
        return new PendingActionPlanBaseline(
            Code,
            Name,
            WindowDays,
            Revision,
            Attempts,
            EndsAt,
            Label,
            EmployeeId,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
