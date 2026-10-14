namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched town hall notice is in its life cycle.</summary>
public enum BatchedTownHallNoticeStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a batched town hall notice and its current status.</summary>
public sealed class BatchedTownHallNotice(string code, string name, double minimum, double weight, string unit, bool pinned, string channel, string jobCode, string externalRef, int endsAt, BatchedTownHallNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public double Weight { get; set; } = weight;
    public string Unit { get; set; } = unit;
    public bool Pinned { get; set; } = pinned;
    public string Channel { get; set; } = channel;
    public string JobCode { get; set; } = jobCode;
    public string ExternalRef { get; set; } = externalRef;
    public int EndsAt { get; set; } = endsAt;
    public BatchedTownHallNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a batched town hall notice in status idle.</summary>
    public static BatchedTownHallNotice NewBatchedTownHallNotice(string code, string name)
    {
        return new BatchedTownHallNotice(
            code,
            name,
            0.0,
            0.0,
            "",
            false,
            "",
            "",
            "",
            0,
            BatchedTownHallNoticeStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTownHallNoticeStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTownHallNoticeStatus next)
    {
        switch (Status)
        {
            case BatchedTownHallNoticeStatus.Idle:
                return next == BatchedTownHallNoticeStatus.Warming;
            case BatchedTownHallNoticeStatus.Warming:
                switch (next)
                {
                    case BatchedTownHallNoticeStatus.Draining:
                        return true;
                }
                return false;
            case BatchedTownHallNoticeStatus.Ready:
                switch (next)
                {
                    case BatchedTownHallNoticeStatus.Draining:
                    case BatchedTownHallNoticeStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedTownHallNoticeStatus.Draining:
                return next == BatchedTownHallNoticeStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BatchedTownHallNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched town hall notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedVolume()
    {
        return Math.Round(Weight + (double)EndsAt, 2);
    }

    public BatchedTownHallNotice Copy()
    {
        return new BatchedTownHallNotice(
            Code,
            Name,
            Minimum,
            Weight,
            Unit,
            Pinned,
            Channel,
            JobCode,
            ExternalRef,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
