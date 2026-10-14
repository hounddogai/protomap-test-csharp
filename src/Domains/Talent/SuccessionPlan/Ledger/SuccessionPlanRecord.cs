namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan record is in its life cycle.</summary>
public enum SuccessionPlanRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A succession plan record. Its code is unique within a tenant.</summary>
public sealed class SuccessionPlanRecord(string code, string name, int startsAt, string channel, int createdAt, double rating, string skill, Dictionary<string, string> attributes, int batchSize, int graceDays, string employeeId, SuccessionPlanRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public string Channel { get; set; } = channel;
    public int CreatedAt { get; set; } = createdAt;
    public double Rating { get; set; } = rating;
    public string Skill { get; set; } = skill;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int BatchSize { get; set; } = batchSize;
    public int GraceDays { get; set; } = graceDays;
    public string EmployeeId { get; set; } = employeeId;
    public SuccessionPlanRecordStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan record in status idle.</summary>
    public static SuccessionPlanRecord NewSuccessionPlanRecord(string code, string name)
    {
        return new SuccessionPlanRecord(
            code,
            name,
            0,
            "",
            0,
            0.0,
            "",
            new Dictionary<string, string>(),
            0,
            0,
            "",
            SuccessionPlanRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanRecordStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanRecordStatus.Idle:
                switch (next)
                {
                    case SuccessionPlanRecordStatus.Ready:
                        return true;
                }
                return false;
            case SuccessionPlanRecordStatus.Warming:
                switch (next)
                {
                    case SuccessionPlanRecordStatus.Stopped:
                        return true;
                }
                return false;
            case SuccessionPlanRecordStatus.Ready:
                switch (next)
                {
                    case SuccessionPlanRecordStatus.Draining:
                    case SuccessionPlanRecordStatus.Stopped:
                        return true;
                }
                return false;
            case SuccessionPlanRecordStatus.Draining:
                return next == SuccessionPlanRecordStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetExposure()
    {
        return Math.Round(Rating * (double)GraceDays, 2);
    }

    public SuccessionPlanRecord Copy()
    {
        return new SuccessionPlanRecord(
            Code,
            Name,
            StartsAt,
            Channel,
            CreatedAt,
            Rating,
            Skill,
            Attributes,
            BatchSize,
            GraceDays,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
