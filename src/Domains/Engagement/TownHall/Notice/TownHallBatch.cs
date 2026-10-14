namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall batch is in its life cycle.</summary>
public enum TownHallBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A town hall batch. Its code is unique within a tenant.</summary>
public sealed class TownHallBatch(string code, string title, double maximum, string employeeId, int updatedAt, TownHallBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string EmployeeId { get; set; } = employeeId;
    public int UpdatedAt { get; set; } = updatedAt;
    public TownHallBatchStatus Status { get; set; } = status;

    /// <summary>Starts a town hall batch in status created.</summary>
    public static TownHallBatch NewTownHallBatch(string code, string title)
    {
        return new TownHallBatch(code, title, 0.0, "", 0, TownHallBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallBatchStatus next)
    {
        switch (Status)
        {
            case TownHallBatchStatus.Created:
                switch (next)
                {
                    case TownHallBatchStatus.Validated:
                        return true;
                }
                return false;
            case TownHallBatchStatus.Validated:
                switch (next)
                {
                    case TownHallBatchStatus.Retired:
                        return true;
                }
                return false;
            case TownHallBatchStatus.Published:
                switch (next)
                {
                    case TownHallBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)UpdatedAt - Maximum, 2);
    }

    public TownHallBatch Copy()
    {
        return new TownHallBatch(Code, Title, Maximum, EmployeeId, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
