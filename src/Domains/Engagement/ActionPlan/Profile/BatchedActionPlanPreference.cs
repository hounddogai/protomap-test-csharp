namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched action plan preference is in its life cycle.</summary>
public enum BatchedActionPlanPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a batched action plan preference and its current status.</summary>
public sealed class BatchedActionPlanPreference(string code, string label, string team, string description, string notes, string employeeId, int tenureMonths, BatchedActionPlanPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string Description { get; set; } = description;
    public string Notes { get; set; } = notes;
    public string EmployeeId { get; set; } = employeeId;
    public int TenureMonths { get; set; } = tenureMonths;
    public BatchedActionPlanPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a batched action plan preference in status created.</summary>
    public static BatchedActionPlanPreference NewBatchedActionPlanPreference(string code, string label)
    {
        return new BatchedActionPlanPreference(
            code,
            label,
            "",
            "",
            "",
            "",
            0,
            BatchedActionPlanPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedActionPlanPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedActionPlanPreferenceStatus next)
    {
        switch (Status)
        {
            case BatchedActionPlanPreferenceStatus.Created:
                switch (next)
                {
                    case BatchedActionPlanPreferenceStatus.Validated:
                    case BatchedActionPlanPreferenceStatus.Retired:
                        return true;
                }
                return false;
            case BatchedActionPlanPreferenceStatus.Validated:
                return next == BatchedActionPlanPreferenceStatus.Published || next == BatchedActionPlanPreferenceStatus.Retired;
            case BatchedActionPlanPreferenceStatus.Published:
                return next == BatchedActionPlanPreferenceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BatchedActionPlanPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched action plan preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"batched action plan preference"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public BatchedActionPlanPreference Copy()
    {
        return new BatchedActionPlanPreference(
            Code,
            Label,
            Team,
            Description,
            Notes,
            EmployeeId,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
