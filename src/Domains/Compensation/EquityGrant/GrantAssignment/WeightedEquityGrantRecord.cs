namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted equity grant record is in its life cycle.</summary>
public enum WeightedEquityGrantRecordStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a weighted equity grant record and its current status.</summary>
public sealed class WeightedEquityGrantRecord(string code, string name, string externalRef, int endsAt, string channel, string phone, int expiresAt, string department, double discount, string reviewer, WeightedEquityGrantRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public int EndsAt { get; set; } = endsAt;
    public string Channel { get; set; } = channel;
    public string Phone { get; set; } = phone;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Department { get; set; } = department;
    public double Discount { get; set; } = discount;
    public string Reviewer { get; set; } = reviewer;
    public WeightedEquityGrantRecordStatus Status { get; set; } = status;

    /// <summary>Starts a weighted equity grant record in status new.</summary>
    public static WeightedEquityGrantRecord NewWeightedEquityGrantRecord(string code, string name)
    {
        return new WeightedEquityGrantRecord(
            code,
            name,
            "",
            0,
            "",
            "",
            0,
            "",
            0.0,
            "",
            WeightedEquityGrantRecordStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedEquityGrantRecordStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedEquityGrantRecordStatus next)
    {
        switch (Status)
        {
            case WeightedEquityGrantRecordStatus.New:
                switch (next)
                {
                    case WeightedEquityGrantRecordStatus.Queued:
                    case WeightedEquityGrantRecordStatus.Running:
                        return true;
                }
                return false;
            case WeightedEquityGrantRecordStatus.Queued:
                return next == WeightedEquityGrantRecordStatus.Failed;
            case WeightedEquityGrantRecordStatus.Running:
                switch (next)
                {
                    case WeightedEquityGrantRecordStatus.Done:
                        return true;
                }
                return false;
            case WeightedEquityGrantRecordStatus.Done:
                return next == WeightedEquityGrantRecordStatus.Failed;
        }
        return false;
    }

    public void MoveTo(WeightedEquityGrantRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted equity grant record {Code} cannot move from {Status} to {next}");
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
        return Math.Round((double)EndsAt + (double)ExpiresAt, 2);
    }

    public WeightedEquityGrantRecord Copy()
    {
        return new WeightedEquityGrantRecord(
            Code,
            Name,
            ExternalRef,
            EndsAt,
            Channel,
            Phone,
            ExpiresAt,
            Department,
            Discount,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
