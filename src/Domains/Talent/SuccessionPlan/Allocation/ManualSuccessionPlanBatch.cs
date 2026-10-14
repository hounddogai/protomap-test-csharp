namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual succession plan batch is in its life cycle.</summary>
public enum ManualSuccessionPlanBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual succession plan batch as the succession plan team keeps it.</summary>
public sealed class ManualSuccessionPlanBatch(string code, string title, int startsAt, string ownerId, string label, string name, ManualSuccessionPlanBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public string OwnerId { get; set; } = ownerId;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public ManualSuccessionPlanBatchStatus Status { get; set; } = status;

    /// <summary>Starts a manual succession plan batch in status created.</summary>
    public static ManualSuccessionPlanBatch NewManualSuccessionPlanBatch(string code, string title)
    {
        return new ManualSuccessionPlanBatch(code, title, 0, "", "", "", ManualSuccessionPlanBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualSuccessionPlanBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualSuccessionPlanBatchStatus next)
    {
        switch (Status)
        {
            case ManualSuccessionPlanBatchStatus.Created:
                return next == ManualSuccessionPlanBatchStatus.Validated;
            case ManualSuccessionPlanBatchStatus.Validated:
                return next == ManualSuccessionPlanBatchStatus.Published || next == ManualSuccessionPlanBatchStatus.Retired;
            case ManualSuccessionPlanBatchStatus.Published:
                switch (next)
                {
                    case ManualSuccessionPlanBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualSuccessionPlanBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual succession plan batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public ManualSuccessionPlanBatch Copy()
    {
        return new ManualSuccessionPlanBatch(Code, Title, StartsAt, OwnerId, Label, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
