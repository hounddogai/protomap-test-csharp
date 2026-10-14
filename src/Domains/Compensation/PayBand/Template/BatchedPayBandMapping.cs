namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched pay band mapping is in its life cycle.</summary>
public enum BatchedPayBandMappingStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A batched pay band mapping. Its code is unique within a tenant.</summary>
public sealed class BatchedPayBandMapping(string code, string name, int capacity, string description, string team, double threshold, string phone, int endsAt, BatchedPayBandMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public string Description { get; set; } = description;
    public string Team { get; set; } = team;
    public double Threshold { get; set; } = threshold;
    public string Phone { get; set; } = phone;
    public int EndsAt { get; set; } = endsAt;
    public BatchedPayBandMappingStatus Status { get; set; } = status;

    /// <summary>Starts a batched pay band mapping in status proposed.</summary>
    public static BatchedPayBandMapping NewBatchedPayBandMapping(string code, string name)
    {
        return new BatchedPayBandMapping(code, name, 0, "", "", 0.0, "", 0, BatchedPayBandMappingStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPayBandMappingStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPayBandMappingStatus next)
    {
        switch (Status)
        {
            case BatchedPayBandMappingStatus.Proposed:
                return next == BatchedPayBandMappingStatus.Approved || next == BatchedPayBandMappingStatus.Withdrawn;
            case BatchedPayBandMappingStatus.Approved:
                return next == BatchedPayBandMappingStatus.Rejected || next == BatchedPayBandMappingStatus.Withdrawn;
            case BatchedPayBandMappingStatus.Rejected:
                switch (next)
                {
                    case BatchedPayBandMappingStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedPayBandMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched pay band mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)EndsAt * (double)Capacity, 2);
    }

    public BatchedPayBandMapping Copy()
    {
        return new BatchedPayBandMapping(Code, Name, Capacity, Description, Team, Threshold, Phone, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
